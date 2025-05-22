using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Bonzer.Propman.App.FacilityTypes;
using Bonzer.Propman.App.Helper;
using Bonzer.Propman.App.Permissions;
using Bonzer.Propman.App.PropertyUnits;
using Bonzer.Propman.App.Settings;
using Bonzer.Propman.App.VisitorInvites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using QRCoder;
using RestSharp;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;

namespace Bonzer.Propman.App.Suprema;

[Authorize(AppPermissions.SupremaPermissions.Default)]
public class SupremaAppService : ApplicationService, ISupremaAppService
{
	private string _apiEndpoint;
	private readonly IFacilityTypesAppService _facilityTypesAppService;
	private readonly IPropertyUnitsAppService _propertyUnitsAppService;
	private readonly RestApiHelper _restApiHelper;
	private readonly IIdentityUserRepository _userRepository;
	private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

	public SupremaAppService(RestApiHelper restApiHelper,
	                         IPropertyUnitsAppService propertyUnitsAppService,
	                         IFacilityTypesAppService facilityTypesAppService, IIdentityUserRepository userRepository,
	                         ICurrentPrincipalAccessor currentPrincipalAccessor)
	{
		_restApiHelper = restApiHelper;
		_propertyUnitsAppService = propertyUnitsAppService;
		_facilityTypesAppService = facilityTypesAppService;
		_userRepository = userRepository;
		_currentPrincipalAccessor = currentPrincipalAccessor;
	}

	private string _bsSessionId { get; set; }


	public virtual async Task<LoginResponse> Login()
	{
		var pollyContext = new Context("Suprema Login Error");
		var policy = Policy
		             .Handle<UserFriendlyException>(ex => ex.Message != String.Empty)
		             .WaitAndRetryAsync(
			             5,
			             _ => TimeSpan.FromMilliseconds(1000),
			             (result, timespan, retryNo, context) =>
			             {
				             Logger.LogError($"{context.OperationKey}: Retry number {retryNo} within " +
				                               $"{timespan.TotalMilliseconds}ms.{result.Message}");
			             }
		             );
		var userName = await SettingProvider.GetOrNullAsync(PropmgtSettingNames.Suprema.Username);
		var password = await SettingProvider.GetOrNullAsync(PropmgtSettingNames.Suprema.Password);
		_apiEndpoint = await SettingProvider.GetOrNullAsync(PropmgtSettingNames.Suprema.Url);

		var request = new RestRequest($"{_apiEndpoint}/api/login");
		var payload = JsonConvert.SerializeObject(new
		{
			User = new
			{
				login_id = userName,
				password = password
			}
		});
		request.AddJsonBody(payload);
		var loginResponse = await policy.ExecuteAsync(async ctx =>
		{
			var response = await _restApiHelper.PostWithRestResponseAsync(request);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new UserFriendlyException(response.Content);
			}
			if (response.Headers == null)
			{
				throw new UserFriendlyException("Suprema Login Response Headers null");
			}

			var headerParam = response.Headers.Where(x => x.Name == "bs-session-id").First();
			_bsSessionId = headerParam.Value.ToString();
			var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(response.Content);
			return loginResponse;
		}, pollyContext);
		
		return loginResponse;
	}


	#region Cards

	public virtual async Task<CardCollectionResponse> GetAllCards()
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/cards");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.GetAsync<CardCollectionResponse>(request);

		return response;
	}

	public virtual async Task<CardCollectionResponse> GetAssignedCards()
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/cards/assigned");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.GetAsync<CardCollectionResponse>(request);

		return response;
	}

	public virtual async Task<CardCollectionResponse> GetUnassignedCards()
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/cards/unassigned");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.GetAsync<CardCollectionResponse>(request);

		return response;
	}

	public virtual async Task<CardTypeCollectionResponse> GetCardTypes()
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/cards/types");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.GetAsync<CardTypeCollectionResponse>(request);

		return response;
	}

	public virtual async Task<CardCollectionResponse> GetCardDetail(string cardId)
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/cards");
		request.AddHeader("bs-session-id", _bsSessionId);
		request.AddParameter("query", cardId);

		var response = await _restApiHelper.GetAsync<CardCollectionResponse>(request);

		return response;
	}

	public virtual async Task<CardCollectionResponse> CreateCard(string qrcode)
	{
		var cardCollectionDto = new CardCollectionCreateDto
		{
			CardCollection = new CardCollectionCreate
			{
				rows = new List<CardCreateDto>
				{
					new()
					{
						card_type = new CardType
						{
							id = "1",
							name = "CSN Wiegand",
							type = "10"
						},
						wiegand_format_id = new WiegandFormatId
						{
							id = "6",
							name = "wiegand 34 QR"
						},
						card_id = qrcode,
						display_card_id = qrcode,
						isDel = false
					}
				}
			}
		};
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/cards");
		request.AddHeader("bs-session-id", _bsSessionId);

		var payload = JsonConvert.SerializeObject(cardCollectionDto,Newtonsoft.Json.Formatting.None, 
			new JsonSerializerSettings { 
				NullValueHandling = NullValueHandling.Ignore
			});
		request.AddJsonBody(payload);

		var response = await _restApiHelper.PostAsync<CardCollectionResponse>(request);
		return response;
	}

	public virtual async Task<ResponseJsonDto> DeleteCard(string cardId)
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/cards");
		request.AddHeader("bs-session-id", _bsSessionId);
		request.AddQueryParameter("id", cardId);
		var response = await _restApiHelper.DeleteAsync(request);
		var responseDto = JsonConvert.DeserializeObject<ResponseJsonDto>(response.Content);
		return responseDto;
	}

	#endregion Cards

	#region AccessGroups

	public virtual async Task<AccessGroupCollectionResponse> GetAccessGroups()
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/access_groups");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.GetAsync<AccessGroupCollectionResponse>(request);
		return response;
	}

	public virtual async Task<AccessGroupResponse> GetAccessGroup(string accessGroupId)
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/access_groups/{accessGroupId}");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.GetAsync<AccessGroupResponse>(request);
		return response;
	}

	public virtual async Task<string> UpdateAccessGroup(string accessGroupId, AccessGroupUpdateDto accessGroupUpdateDto)
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/access_groups/{accessGroupId}");
		request.AddHeader("bs-session-id", _bsSessionId);

		request.AddJsonBody(JsonConvert.SerializeObject(accessGroupUpdateDto,Newtonsoft.Json.Formatting.None, 
			new JsonSerializerSettings { 
				NullValueHandling = NullValueHandling.Ignore
			}));

		var response = await _restApiHelper.PutAsync(request);
		return response.Content;
	}

	public virtual async Task<List<AccessGroupDto>> GetVisitorAccessGroups(string blkName, string level)
	{
		var response = await GetAccessGroups();
		var accessGroupDtos = response.AccessGroupCollection.rows.Where(x =>
			                              x.name.ToUpper() == blkName.ToUpper() + " B1 COMMON LIFT LOBBY QR" ||
			                              x.name.ToUpper() == blkName.ToUpper() + " L1 COMMON LIFT LOBBY QR" || 
			                              x.name.ToUpper() == blkName.ToUpper() + " LEVEL " +  level + " QR")
		                              .ToList();
		return accessGroupDtos;
	}

	public virtual async Task<ResponseJsonDto> DeleteAccessGroup(string accessGroupId)
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/access_groups");
		request.AddHeader("bs-session-id", _bsSessionId);
		request.AddQueryParameter("id", accessGroupId);
		var response = await _restApiHelper.DeleteAsync<ResponseJsonDto>(request);
		return response;
	}

	public virtual async Task<ResponseJsonDto> CreateAccessGroup()
	{
		var accessGroupCreateDto = new AccessGroupCreateDto
		{
			AccessGroup = new AccessGroup
			{
				name = "Test AG"

				//access_levels = new List<IdDto> { },
				//user_groups = new List<IdDto> { },
				//users = new List<UserIdDto> { },
			}
		};
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/access_groups");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.PostAsync<ResponseJsonDto>(request);
		return response;
	}

	#endregion AccessGroups

	#region User Groups

	public virtual async Task<UserGroupCollectionResponse> GetUserGroups()
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/user_groups");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.GetAsync<UserGroupCollectionResponse>(request);
		return response;
	}

	public virtual async Task<List<UserGroupDto>> GetUserGroupsByUnitName(string blkName, string unitName)
	{
		var userGroupsResponse = await GetUserGroups();

		var usergroups = userGroupsResponse.UserGroupCollection.rows
		                                   .Where(x => x.name == unitName &&
		                                               x.parent_id.name.ToUpper() == blkName.ToUpper()).ToList();

		return usergroups;
	}

	public virtual async Task<List<UserGroupDto>> GetGuestUserGroupsByUnitName(string blkName, string unitName)
	{
		var userGroupsResponse = await GetUserGroups();

		var usergroups = userGroupsResponse.UserGroupCollection.rows.Where(x =>
			                                   x.name.ToUpper() == unitName + " GUEST" &&
			                                   x.parent_id.name.ToUpper() == blkName.ToUpper() + " GUEST")
		                                   .ToList();

		return usergroups;
	}

	public virtual async Task<ResponseJsonDto> CreateUserGroup()
	{
		var userGroupCreateDto = new UserGroupCreateDto
		{
			UserGroup = new UserGroupCreate
			{
				depth = 1,
				name = "Test UG",
				parent_id = new IdDto
				{
					id = 1
				}
			}
		};
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/user_groups");
		request.AddHeader("bs-session-id", _bsSessionId);
		request.AddJsonBody(JsonConvert.SerializeObject(userGroupCreateDto, Newtonsoft.Json.Formatting.None, 
			new JsonSerializerSettings { 
				NullValueHandling = NullValueHandling.Ignore
			}));

		var response = await _restApiHelper.PostAsync<ResponseJsonDto>(request);
		return response;
	}

	public virtual async Task<ResponseJsonDto> DeleteUserGroup(string userGroupId)
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/user_groups");
		request.AddHeader("bs-session-id", _bsSessionId);
		request.AddQueryParameter("id", userGroupId);
		var response = await _restApiHelper.DeleteAsync<ResponseJsonDto>(request);
		return response;
	}

	#endregion User Groups

	#region Users

	public virtual async Task<UserCollectionResponse> GetUsers()
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/users");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.GetAsync<UserCollectionResponse>(request);
		return response;
	}

	public virtual async Task<UserCollectionResponse> CreateUser(UserCreateUpdateDto userCreateUpdateDto)
	{
		//var userDto = new UserCreateDto()
		//{
		//    User = new UserCreate()
		//    {
		//        access_groups = new List<IdDto> { new IdDto() { id = 1 } },
		//        cards = new List<IdDto> { new IdDto() { id = 1 } },
		//        user_group_id = new IdDto() { id = 1 },
		//        disabled = false,
		//        name = "Test Vistor",
		//        start_datetime = DateTime.Now,
		//        expiry_datetime = DateTime.Now.AddDays(1),
		//        user_id = "1",
		//    }
		//};
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/users");
		request.AddHeader("bs-session-id", _bsSessionId);
		var options = new JsonSerializerOptions
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
		var payload = JsonConvert.SerializeObject(
			userCreateUpdateDto, 
			Newtonsoft.Json.Formatting.None, 
			new JsonSerializerSettings { 
				NullValueHandling = NullValueHandling.Ignore
			});
		request.AddJsonBody(payload);

		try
		{
			var response = await _restApiHelper.PostAsync<UserCollectionResponse>(request);

			if (response == null)
			{
				throw new UserFriendlyException("[Suprema Create User] Response Null");
			}
			
			return response;
		}
		catch (Exception ex)
		{
			Logger.LogError($"bs-session-id : {_bsSessionId}");
			Logger.LogError($"UserCreateDto : \n{JsonConvert.SerializeObject(userCreateUpdateDto,Formatting.Indented)}");
			throw ex;
		}
	}

	public virtual async Task<UserUpdateResponse> UpdateUser(UserCreateUpdateDto userCreateUpdateDto)
	{
		try
		{
			await Login();
			var request = new RestRequest($"{_apiEndpoint}/api/users");
			request.AddQueryParameter("id", userCreateUpdateDto.User.user_id);
			request.AddHeader("bs-session-id", _bsSessionId);
			var payload = JsonConvert.SerializeObject(userCreateUpdateDto,new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			});
			request.AddJsonBody(payload);

		
			var response = await _restApiHelper.PutAsync<UserUpdateResponse>(request);

			if (response == null)
			{
				throw new UserFriendlyException("[Suprema Update User] Response Null");
			}
			
			return response;
		}
		catch (Exception ex)
		{
			Logger.LogError($"bs-session-id : {_bsSessionId}");
			Logger.LogError($"UserUpdateDto : \n{JsonConvert.SerializeObject(userCreateUpdateDto,Formatting.Indented)}");
			throw ex;
		}
	}

	public virtual async Task<ResponseJsonDto> DeleteUser(string userId)
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/users");
		request.AddHeader("bs-session-id", _bsSessionId);
		request.AddQueryParameter("id", userId);
		var response = await _restApiHelper.DeleteAsync(request);

		var responseDto = JsonConvert.DeserializeObject<ResponseJsonDto>(response.Content);

		return responseDto;
	}

	public virtual async Task<ResponseJsonDto> AssignCardToUser(string userId, string cardId)
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/users");
		request.AddHeader("bs-session-id", _bsSessionId);
		request.AddQueryParameter("id", userId);
		var payload = JsonConvert.SerializeObject(new
		{
			User = new
			{
				cards = new[] { new { id = cardId } }.ToList()
			}
		});
		request.AddJsonBody(payload);
		try
		{
			var response = await _restApiHelper.PutAsync(request);

			if (response.Content == null)
			{
				throw new UserFriendlyException($"[Suprema Assign Card To User] Response content null. Response status code : {response.StatusCode}");
			}

			var responseDto = JsonConvert.DeserializeObject<ResponseJsonDto>(response.Content);
			return responseDto;
		}
		catch (Exception ex)
		{
			Logger.LogError($"bs-session-id : {_bsSessionId}");
			Logger.LogError($"UserId : {userId}");
			Logger.LogError($"Payload : \n{JsonConvert.SerializeObject(new
			{
				User = new
				{
					cards = new[] { new { id = cardId } }.ToList()
				}
			},Formatting.Indented)}");
			throw ex;
		}
	}

	public virtual async Task<GetNextAvailableUserIdResponse> GetNextAvailableUserId()
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/users/next_user_id");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.GetAsync<GetNextAvailableUserIdResponse>(request);
		return response;
	}

	#endregion Users

	#region Visitors

	public virtual async Task<ResponseJsonDto> EnrollVisitor(VisitorInviteDto visitorInviteDto)
	{
		//create card with qrcode
		var isNumeric = int.TryParse(visitorInviteDto.AccessQrCode, out _);
		if (!isNumeric)
		{
			throw new UserFriendlyException("Invalid Access QrCode. QrCode must only contain numbers.");
		}

		if (visitorInviteDto.AccessQrCode.Length > 10)
		{
			throw new UserFriendlyException("Invalid Access QrCode. QrCode length cannot be more than 10 digits.");
		}

		var cardCreateResponse = await CreateCard(visitorInviteDto.AccessQrCode);
		var cardId = cardCreateResponse.CardCollection.rows.First().id;
		if (cardCreateResponse.Response.code == "0")
		{
			Logger.LogInformation($"Visitor Card({cardId}) Created");
		}
		else
		{
			throw new UserFriendlyException($"{cardCreateResponse.Response.message}");
		}

		//get property unit and block
		var propUnitDto =
			await _propertyUnitsAppService.GetWithNavigationPropertiesAsync(visitorInviteDto.PropertyUnitId);
		//get property unit level
		var level = propUnitDto.PropertyUnit.UnitNo.Replace("#","").Replace("-","").Substring(0, 2);
		//get access groups
		var accessGroups = await GetVisitorAccessGroups(propUnitDto.PropertyBlock.BlockName, level);
		if (accessGroups.Count == 0)
		{
			throw new UserFriendlyException($"No AccessGroups found for {propUnitDto.PropertyBlock.BlockName}");
		}

		//get user groups
		// var userGroups =
		// 	await GetGuestUserGroupsByUnitName(propUnitDto.PropertyBlock.BlockName,
		// 		propUnitDto.PropertyUnit.UnitNo);
		// if (userGroups.Count == 0)
		// {
		// 	throw new UserFriendlyException(
		// 		$"No UserGroups found for {propUnitDto.PropertyBlock.BlockName} {propUnitDto.PropertyUnit.UnitNo}");
		// }
		//
		// if (userGroups.Count != 1)
		// {
		// 	throw new UserFriendlyException(
		// 		$"More than 1 UserGroups found for {propUnitDto.PropertyBlock.BlockName} {propUnitDto.PropertyUnit.UnitNo}");
		// }

		//create user
		//get latest user id
		var userCollectionResponse = await GetUsers();
		var newUserIdResponse = await GetNextAvailableUserId();
		var newUserId = newUserIdResponse.User.user_id;
		var visitorName = propUnitDto.PropertyUnit.NormalizedBlockUnit + " Visitor " + visitorInviteDto.VisitorName;
		if (visitorName.Length > 48)
		{
			var maxLength = 48 - (propUnitDto.PropertyUnit.NormalizedBlockUnit + " Visitor ").Length;
			throw new UserFriendlyException($"Visitor Name ({visitorInviteDto.VisitorName}) cannot be more than {maxLength} characters!");
		}
		
		var userCreateDto = new UserCreateUpdateDto
		{
			User = new UserCreateUpdate
			{
				access_groups = accessGroups.Select(x => new IdDto() { id = Int32.Parse(x.id) }).ToList(),
				user_group_id = new IdDto
				{
					id = 1
				},
				disabled = false,
				name = visitorName,
				start_datetime = visitorInviteDto.ScheduledStartDate,
				expiry_datetime = visitorInviteDto.ScheduledEndDate,
				user_id = newUserId.ToString()
			}
		};

		var createUserResponse = await CreateUser(userCreateDto);
		if (createUserResponse == null)
		{
			throw new UserFriendlyException($"Error enrolling Visitor {visitorName} for Suprema");
		}

		if (createUserResponse.Response.code != "0")
		{
			throw new UserFriendlyException($"{createUserResponse.Response.message}");
		}

		//assign card to user
		var assignCardResponse = await AssignCardToUser(newUserId.ToString(), cardId);
		if (assignCardResponse.Response.code != "0")
		{
			throw new UserFriendlyException($"{assignCardResponse.Response.message}");
		}

		return assignCardResponse;
	}

	public virtual async Task<ResponseJsonDto> UpdateVisitor(VisitorInviteDto visitorInviteDto)
	{
		//find card by qrcode
		if (visitorInviteDto.AccessQrCode == null || visitorInviteDto.AccessQrCode == String.Empty)
		{
			throw new UserFriendlyException($"Qr Code is null or empty for visitor!");
		}

		var cardCollectionResponse = await GetCardDetail(visitorInviteDto.AccessQrCode);
		if (cardCollectionResponse.CardCollection.rows.Count == 0)
		{
			throw new UserFriendlyException($"No Cards found for Qr Code ({visitorInviteDto.AccessQrCode})");
		}

		if (cardCollectionResponse.CardCollection.rows.Count > 1)
		{
			throw new UserFriendlyException(
				$"More than 1 Card found for Qr Code ({visitorInviteDto.AccessQrCode})");
		}

		var cardDto = cardCollectionResponse.CardCollection.rows.First();
		var cardId = cardDto.id;

		//delete user
		if (cardDto.user_id != null)
		{
			var deleteUserResponse =
				await DeleteUser(cardCollectionResponse.CardCollection.rows.First().user_id.user_id);
			if (deleteUserResponse.Response.code != "0")
			{
				throw new UserFriendlyException($"{deleteUserResponse.Response.message}");
			}
		}

		//get property unit and block
		var propUnitDto =
			await _propertyUnitsAppService.GetWithNavigationPropertiesAsync(visitorInviteDto.PropertyUnitId);

		//get access groups
		//var accessGroups = await GetVisitorAccessGroups(propUnitDto.PropertyBlock.BlockName);
		//if (accessGroups.Count == 0)
		//{
		//    throw new UserFriendlyException($"No AccessGroups found for {propUnitDto.PropertyBlock.BlockName}");
		//}
		//get user groups
		var userGroups =
			await GetGuestUserGroupsByUnitName(propUnitDto.PropertyBlock.BlockName,
				propUnitDto.PropertyUnit.UnitNo);
		if (userGroups.Count == 0)
		{
			throw new UserFriendlyException(
				$"No UserGroups found for {propUnitDto.PropertyBlock.BlockName} {propUnitDto.PropertyUnit.UnitNo}");
		}

		if (userGroups.Count != 1)
		{
			throw new UserFriendlyException(
				$"More than 1 UserGroups found for {propUnitDto.PropertyBlock.BlockName} {propUnitDto.PropertyUnit.UnitNo}");
		}

		//create user
		//get latest user id
		var userCollectionResponse = await GetUsers();
		var newUserIdResponse = await GetNextAvailableUserId();
		var newUserId = newUserIdResponse.User.user_id;
		var userCreateDto = new UserCreateUpdateDto
		{
			User = new UserCreateUpdate
			{
				//access_groups = accessGroups.Select(x => new IdDto() { id = Int32.Parse(x.id) }).ToList(),
				user_group_id = userGroups.Select(x => new IdDto { id = int.Parse(x.id) }).First(),
				disabled = false,
				name = visitorInviteDto.VisitorName,
				start_datetime = visitorInviteDto.ScheduledStartDate,
				expiry_datetime = visitorInviteDto.ScheduledEndDate,
				user_id = newUserId.ToString()
			}
		};

		var createUserResponse = await CreateUser(userCreateDto);
		if (createUserResponse.Response.code != "0")
		{
			throw new UserFriendlyException($"{createUserResponse.Response.message}");
		}

		//assign card to user
		var assignCardResponse = await AssignCardToUser(newUserId.ToString(), cardId);
		if (assignCardResponse.Response.code != "0")
		{
			throw new UserFriendlyException($"{assignCardResponse.Response.message}");
		}

		return assignCardResponse;
	}

	public virtual async Task<ResponseJsonDto> DeleteUserAndCardByQrCode(string accessQrCode)
	{
		//find card by qrcode
		if (accessQrCode == null || accessQrCode == String.Empty)
		{
			throw new UserFriendlyException($"Qr Code is null or empty!");
		}

		var cardCollectionResponse = await GetCardDetail(accessQrCode);
		if (cardCollectionResponse == null || cardCollectionResponse.CardCollection == null)
		{
			throw new UserFriendlyException($"Card Collection null. No Cards found for Qr Code ({accessQrCode})");
		}

		if (cardCollectionResponse.CardCollection.rows.Count == 0)
		{
			throw new UserFriendlyException($"No Cards found for Qr Code ({accessQrCode})");
		}

		if (cardCollectionResponse.CardCollection.rows.Count > 1)
		{
			throw new UserFriendlyException($"More than 1 Card found for Qr Code ({accessQrCode})");
		}

		var cardId = cardCollectionResponse.CardCollection.rows.First().id;

		//delete user
		if (cardCollectionResponse.CardCollection.rows.First().user_id != null)
		{
			var deleteUserResponse =
				await DeleteUser(cardCollectionResponse.CardCollection.rows.First().user_id.user_id);
			if (deleteUserResponse.Response.code != "0")
			{
				throw new UserFriendlyException($"{deleteUserResponse.Response.message}");
			}
		}

		//delete card
		var deleteCardResponse = await DeleteCard(cardId);
		if (deleteCardResponse.Response.code != "0")
		{
			throw new UserFriendlyException($"{deleteCardResponse.Response.message}");
		}

		return deleteCardResponse;
	}


	public virtual async Task RemoveVisitorFromAccessGroup(string userId, string accessGroupId)
	{
		var accessGroupResponse = await GetAccessGroup(accessGroupId);
		if (accessGroupResponse == null)
		{
			throw new UserFriendlyException($"Access Group {accessGroupId} not found");
		}

		if (accessGroupResponse.AccessGroup.users == null ||
		    !accessGroupResponse.AccessGroup.users.Any(x => x.user_id.ToString() == userId))
		{
			throw new UserFriendlyException(
				$"User Id {userId} not found in Access Group [{accessGroupResponse.AccessGroup.name}]");
		}

		var accessGroupUpdateDto = new AccessGroupUpdateDto()
		{
			AccessGroup = new AccessGroupUpdate
			{
				name = accessGroupResponse.AccessGroup.name,
				description = accessGroupResponse.AccessGroup.description,
				users = accessGroupResponse.AccessGroup.users.Where(x => x.user_id.ToString() != userId)
				                           .ToList(),
				delete_users = new List<UserIdDto>()
				{
					new UserIdDto
					{
						user_id = Convert.ToInt32(userId)
					}
				},
				user_groups = accessGroupResponse.AccessGroup.user_groups,
				access_levels = accessGroupResponse.AccessGroup.access_levels,
				floor_levels = accessGroupResponse.AccessGroup.floor_levels.Select(x => new IdDto
				{
					id = Convert.ToInt32(x.id)
				}).ToList()
			}
		};

		await UpdateAccessGroup(accessGroupId, accessGroupUpdateDto);
	}

	#endregion Visitors

	#region Facility

	private string GetSupremaAccessGroupName(string facilityTypeName)
	{
		var accessGroupName = string.Empty;
		if (facilityTypeName == "The Dining Hall 1")
		{
			accessGroupName = "The Dining Hall 1";
		}
		else if (facilityTypeName.Contains("The Dining Hall 2"))
		{
			accessGroupName = "The Dining Hall 2";
		}
		else if (facilityTypeName.Contains("The Dining Lounge 1"))
		{
			accessGroupName = "The Dining Lounge 1";
		}
		else if (facilityTypeName.Contains("The Dining Lounge 2"))
		{
			accessGroupName = "The Dining Lounge 2";
		}
		else if (facilityTypeName.Contains("The Sky Villa"))
		{
			accessGroupName = "The Sky Villa";
		}
		else if (facilityTypeName.Contains("The Mountbatten Hall"))
		{
			accessGroupName = "The Mountbatten Hall";
		}
		else if (facilityTypeName.Contains("The Mountbatten Villa"))
		{
			accessGroupName = "The Mountbatten Villa";
		}

		return accessGroupName;
	}
	[AllowAnonymous]
	public virtual async Task<List<UserUpdateResponse>> AddUserToFacilityAccessGroup(Guid facilityTypeId, Guid propUnitId)
	{
		var adminPrincipal = new ClaimsPrincipal(
			new ClaimsIdentity(
				new Claim[]
				{
					//new Claim(AbpClaimTypes.UserId, Guid.NewGuid().ToString()),
					new Claim(AbpClaimTypes.UserName, "admin"),
					new Claim(AbpClaimTypes.Role, "admin"),
				}
			)
		);

		using (_currentPrincipalAccessor.Change(adminPrincipal))
		{
			//get access group from facility
			var facilityTypeDto = await _facilityTypesAppService.GetAsync(facilityTypeId);

			// var accessGroupName = GetSupremaAccessGroupName(facilityTypeDto.Facility);
			var accessGroupName = facilityTypeDto.Facility;
			var updateResponses = new List<UserUpdateResponse>();

			if (accessGroupName == String.Empty)
			{
				Logger.LogError($"Facility {facilityTypeDto.Facility} has no mapping to Suprema Access group");
				updateResponses.Add(new UserUpdateResponse
				{
					FailedUserCollection = null,
					Response = new ResponseDto
					{
						code = "0",
						link = null,
						message = $"Facility {facilityTypeDto.Facility} has no mapping to Suprema Access group"
					}
				});
				return updateResponses;
			}

			var accessGroupCollectionResponse = await GetAccessGroups();
			var accessGroup = accessGroupCollectionResponse.AccessGroupCollection.rows
			                                               .Where(x => x.name.ToUpper() == accessGroupName.ToUpper())
			                                               .FirstOrDefault();
			if (accessGroup == null)
			{
				updateResponses.Add(new UserUpdateResponse
				{
					FailedUserCollection = null,
					Response = new ResponseDto
					{
						code = "0",
						link = null,
						message = $"Suprema Access Group {facilityTypeDto.Facility} not found"
					}
				});
				return updateResponses;

				// throw new UserFriendlyException($"{accessGroupName} AccessGroup not found.");
			}

			//get user
			// var userDto = await _userRepository.GetAsync(userId);

			//get prop unit id
			// var propUnitId = userDto.GetProperty<string>("PropertyUnitId");
			// if (propUnitId == string.Empty || propUnitId == null)
			// {
			// 	throw new UserFriendlyException($"Property Unit Id not found on User {userDto.UserName}!");
			// }

			var propertyUnitWithNavigationPropertiesDto =
				await _propertyUnitsAppService.GetWithNavigationPropertiesAsync(propUnitId);

			//get suprema user group by unit no
			var userGroupCollectionResponse = await GetUserGroups();
			var supremaUserGroup = userGroupCollectionResponse.UserGroupCollection.rows.Where(x =>
				                                                  x.name.Trim().ToUpper() ==
				                                                  propertyUnitWithNavigationPropertiesDto.PropertyUnit
					                                                  .UnitNo.Trim().ToUpper())
			                                                  .FirstOrDefault();
			if (supremaUserGroup == null)
			{
				throw new UserFriendlyException(
					$"Suprema UserGroup {propertyUnitWithNavigationPropertiesDto.PropertyUnit.UnitNo} not found");
			}

			//get suprema users from usergroup
			var userCollectionResponse = await GetUsers();
			var supremaUsers = userCollectionResponse.UserCollection.rows.Where(x =>
				x.user_group_id.id == supremaUserGroup.id).ToList();

			if (supremaUsers.Count == 0)
			{
				throw new UserFriendlyException(
					$"Suprema Users for UserGroup {propertyUnitWithNavigationPropertiesDto.PropertyUnit.UnitNo} not found");
			}


			foreach (var supremaUser in supremaUsers)
			{
				//check if user is assigned to facility access group
				if (supremaUser.access_groups != null && supremaUser.access_groups.Any(x => x.id == accessGroup.id))
				{
					continue;
				}

				//Add new access group
				var supremaUserAccessGroups = new List<IdDto>();
				if (supremaUser.access_groups != null)
				{
					supremaUserAccessGroups = supremaUser.access_groups.Select(x => new IdDto()
					{
						id = Int32.Parse(x.id)
					}).ToList();
				}

				supremaUserAccessGroups.Add(new IdDto
				{
					id = Int32.Parse(accessGroup.id),
				});

				var updateUser = new UserCreateUpdateDto
				{
					User = new UserCreateUpdate
					{
						name = supremaUser.name,
						user_id = supremaUser.user_id,
						user_group_id = new IdDto()
						{
							id = Int32.Parse(supremaUser.user_group_id.id)
						},
						disabled = Boolean.Parse(supremaUser.disabled),
						start_datetime = supremaUser.start_datetime,
						expiry_datetime = supremaUser.expiry_datetime,
						access_groups = supremaUserAccessGroups,
						permission = supremaUser.permission != null ? Int32.Parse(supremaUser.permission.id) : null,
						email = supremaUser.email,
						login_id = supremaUser.login_id,
					}
				};
				var updateResponse = await UpdateUser(updateUser);

				updateResponses.Add(updateResponse);

			}

			return updateResponses;
		}
	}
	[AllowAnonymous]
	public virtual async Task<List<UserUpdateResponse>> RemoveUserFromFacilityAccessGroup(Guid facilityTypeId, Guid propUnitId)
	{
		var adminPrincipal = new ClaimsPrincipal(
			new ClaimsIdentity(
				new Claim[]
				{
					//new Claim(AbpClaimTypes.UserId, Guid.NewGuid().ToString()),
					new Claim(AbpClaimTypes.UserName, "admin"),
					new Claim(AbpClaimTypes.Role, "admin"),
				}
			)
		);

		using (_currentPrincipalAccessor.Change(adminPrincipal))
		{
			//get access group from facility
			var facilityTypeDto = await _facilityTypesAppService.GetAsync(facilityTypeId);
			// var accessGroupName = GetSupremaAccessGroupName(facilityTypeDto.Facility);
			var accessGroupName = facilityTypeDto.Facility;

			var updateResponses = new List<UserUpdateResponse>();

			if (accessGroupName == String.Empty)
			{
				Logger.LogError($"Facility {facilityTypeDto.Facility} has no mapping to Suprema Access group");
				updateResponses.Add(new UserUpdateResponse
				{
					FailedUserCollection = null,
					Response = new ResponseDto
					{
						code = "0",
						link = null,
						message = $"Facility {facilityTypeDto.Facility} has no mapping to Suprema Access group"
					}
				});
				return updateResponses;
			}

			var accessGroupCollectionResponse = await GetAccessGroups();
			var accessGroup = accessGroupCollectionResponse.AccessGroupCollection.rows
			                                               .Where(x => x.name.ToUpper() == accessGroupName.ToUpper())
			                                               .FirstOrDefault();
			if (accessGroup == null)
			{
				throw new UserFriendlyException($"{accessGroupName} AccessGroup not found.");
			}

			//get user
			// var userDto = await _userRepository.GetAsync(userId);

			//get prop unit id
			// var propUnitId = userDto.GetProperty<string>("PropertyUnitId");
			// if (propUnitId == string.Empty || propUnitId == null)
			// {
			// 	throw new UserFriendlyException($"Property Unit Id not found on User {userDto.UserName}!");
			// }

			var propertyUnitWithNavigationPropertiesDto =
				await _propertyUnitsAppService.GetWithNavigationPropertiesAsync(propUnitId);

			//get suprema user group by unit no
			var userGroupCollectionResponse = await GetUserGroups();
			var supremaUserGroup = userGroupCollectionResponse.UserGroupCollection.rows.Where(x =>
				                                                  x.name.Trim().ToUpper() ==
				                                                  propertyUnitWithNavigationPropertiesDto.PropertyUnit
					                                                  .UnitNo.Trim().ToUpper())
			                                                  .FirstOrDefault();
			if (supremaUserGroup == null)
			{
				throw new UserFriendlyException(
					$"Suprema UserGroup {propertyUnitWithNavigationPropertiesDto.PropertyUnit.UnitNo} not found");
			}

			//get suprema users from usergroup
			var userCollectionResponse = await GetUsers();
			var supremaUsers = userCollectionResponse.UserCollection.rows.Where(x =>
				x.user_group_id.id == supremaUserGroup.id).ToList();

			if (supremaUsers.Count == 0)
			{
				throw new UserFriendlyException(
					$"Suprema Users for UserGroup {propertyUnitWithNavigationPropertiesDto.PropertyUnit.UnitNo} not found");
			}

			foreach (var supremaUser in supremaUsers)
			{
				//check if user is not assigned to facility access group
				if (supremaUser.access_groups != null && supremaUser.access_groups.All(x => x.id != accessGroup.id))
				{
					continue;
				}

				if (supremaUser.access_groups == null)
				{
					continue;
				}

				//Remove access group
				var supremaUserAccessGroups = new List<IdDto>();

				supremaUserAccessGroups = supremaUser.access_groups.Where(x => x.id != accessGroup.id).Select(x =>
					new IdDto()
					{
						id = Int32.Parse(x.id)
					}).ToList();


				var updateUser = new UserCreateUpdateDto
				{
					User = new UserCreateUpdate
					{
						name = supremaUser.name,
						user_id = supremaUser.user_id,
						user_group_id = new IdDto()
						{
							id = Int32.Parse(supremaUser.user_group_id.id)
						},
						disabled = Boolean.Parse(supremaUser.disabled),
						start_datetime = supremaUser.start_datetime,
						expiry_datetime = supremaUser.expiry_datetime,
						access_groups = supremaUserAccessGroups,
						permission = supremaUser.permission != null ? Int32.Parse(supremaUser.permission.id) : null,
						email = supremaUser.email,
						login_id = supremaUser.login_id,
					}
				};
				var updateResponse = await UpdateUser(updateUser);
				updateResponses.Add(updateResponse);
			}

			return updateResponses;
		}
	}
	public virtual async Task CreateFacilityCardForUser(Guid facilityTypeId, Guid userId, DateTime startTime,
	                                                    DateTime endTime, string qrcode)
	{
		//get access group from facility
		var facilityTypeDto = await _facilityTypesAppService.GetAsync(facilityTypeId);
		var accessGroupName = string.Empty;

		if (facilityTypeDto.Facility == "The Dining Lounge")
		{
			accessGroupName = "The Dining Lounge";
		}
		else if (facilityTypeDto.Facility == "Gourmet Pavilion")
		{
			accessGroupName = "Gourmet Pavilion";
		}
		else if (facilityTypeDto.Facility == "Veranda")
		{
			accessGroupName = "Veranda";
		}
		else if (facilityTypeDto.Facility == "The Dining Hall 1")
		{
			accessGroupName = "The Dining Hall 1";
		}
		else if (facilityTypeDto.Facility.Contains("The Dining Hall 2"))
		{
			accessGroupName = "The Dining Hall 2";
		}
		else if (facilityTypeDto.Facility.Contains("The Dining Lounge 1"))
		{
			accessGroupName = "The Dining Lounge 1";
		}
		else if (facilityTypeDto.Facility.Contains("The Dining Lounge 2"))
		{
			accessGroupName = "The Dining Lounge 2";
		}
		else if (facilityTypeDto.Facility.Contains("The Sky Grill"))
		{
			accessGroupName = "The Sky Grill";
		}
		else if (facilityTypeDto.Facility.Contains("The Sky Villa"))
		{
			accessGroupName = "The Sky Villa";
		}
		else if (facilityTypeDto.Facility.Contains("The Grill"))
		{
			accessGroupName = "The Grill";
		}
		else if (facilityTypeDto.Facility.Contains("The Mountbatten Hall"))
		{
			accessGroupName = "The Mountbatten Hall";
		}
		else if (facilityTypeDto.Facility.Contains("The Mountbatten Grill"))
		{
			accessGroupName = "The Mountbatten Grill";
		}
		else if (facilityTypeDto.Facility.Contains("The Mountbatten Villa"))
		{
			accessGroupName = "The Mountbatten Villa";
		}
		else
		{
			return;

			//throw new UserFriendlyException($"No access group handled for {facilityTypeDto.Facility}!");
		}

		var accessGroupCollectionResponse = await GetAccessGroups();
		var accessGroup = accessGroupCollectionResponse.AccessGroupCollection.rows
		                                               .Where(x => x.name.ToUpper() == accessGroupName.ToUpper())
		                                               .FirstOrDefault();
		if (accessGroup == null)
		{
			throw new UserFriendlyException($"{accessGroupName} AccessGroup not found.");
		}

		//create user
		var userDto = await _userRepository.GetAsync(userId);

		//get user group
		var propUnitId = userDto.GetProperty<string>("PropertyUnitId");
		if (propUnitId == string.Empty || propUnitId == null)
		{
			throw new UserFriendlyException($"Property Unit Id not found on User {userDto.UserName}!");
		}

		var propertyUnitWithNavigationPropertiesDto =
			await _propertyUnitsAppService.GetWithNavigationPropertiesAsync(Guid.Parse(propUnitId));
		var userGroupDtos = await GetUserGroupsByUnitName(
			propertyUnitWithNavigationPropertiesDto.PropertyBlock.BlockName,
			propertyUnitWithNavigationPropertiesDto.PropertyUnit.UnitNo);
		if (userGroupDtos.Count == 0)
		{
			throw new UserFriendlyException(
				$"Biostar Usergroup for {propertyUnitWithNavigationPropertiesDto.PropertyBlock.BlockName} {propertyUnitWithNavigationPropertiesDto.PropertyUnit.UnitNo} not found!");
		}

		var userGroupDto = userGroupDtos.First();

		var newUserIdResponse = await GetNextAvailableUserId();
		var newUserId = newUserIdResponse.User.user_id.ToString();
		var userCreateDto = new UserCreateUpdateDto
		{
			User = new UserCreateUpdate
			{
				access_groups = new List<IdDto> { new() { id = int.Parse(accessGroup.id) } },
				user_group_id = new IdDto { id = int.Parse(userGroupDto.id) },
				disabled = false,
				name = userDto.UserName,
				start_datetime = startTime,
				expiry_datetime = endTime,
				user_id = newUserId
			}
		};

		var createUserResponse = await CreateUser(userCreateDto);
		if (createUserResponse.Response.code != "0")
		{
			throw new UserFriendlyException($"{createUserResponse.Response.message}");
		}

		//create card
		//generate random 9 digit qrcode, max 10 digits
		var generator = new Random();

		//var qrcode = generator.Next(100000000, 1000000000).ToString("D9");
		var cardCreateResponse = await CreateCard(qrcode);
		var cardId = cardCreateResponse.CardCollection.rows.First().id;
		if (cardCreateResponse.Response.code == "0")
		{
			Logger.LogInformation($"Facility Card({cardId}) Created");
		}
		else
		{
			throw new UserFriendlyException($"{cardCreateResponse.Response.message}");
		}

		//link card with user
		var assignCardResponse = await AssignCardToUser(newUserId, cardId);
		if (assignCardResponse.Response.code != "0")
		{
			throw new UserFriendlyException($"{assignCardResponse.Response.message}");
		}
	}

	public virtual async Task<QrCodeDto> GetFacilityCardForUser(Guid facilityTypeId, Guid userId)
	{
		var qrcode = string.Empty;

		//get access group from facility
		var facilityTypeDto = await _facilityTypesAppService.GetAsync(facilityTypeId);
		var accessGroupName = string.Empty;

		if (facilityTypeDto.Facility == "The Boardroom")
		{
			accessGroupName = "Clubhouse The Boardroom";
		}
		else if (facilityTypeDto.Facility == "Bakerzone")
		{
			accessGroupName = "Clubhouse Baker Zone";
		}
		else if (facilityTypeDto.Facility == "The Bar and The Den ")
		{
			accessGroupName = "Clubhouse The Den";
		}
		else if (facilityTypeDto.Facility == "Party Lounge")
		{
			accessGroupName = "Clubhouse Party Lounge";
		}
		else if (facilityTypeDto.Facility.Contains("Fitness X"))
		{
			accessGroupName = "Clubhouse Fitness X";
		}
		else
		{
			throw new UserFriendlyException($"No access group handled for {facilityTypeDto.Facility}!");
		}

		var accessGroupCollectionResponse = await GetAccessGroups();
		var accessGroup = accessGroupCollectionResponse.AccessGroupCollection.rows
		                                               .Where(x => x.name.ToUpper() == accessGroupName.ToUpper())
		                                               .FirstOrDefault();
		if (accessGroup == null)
		{
			throw new UserFriendlyException($"{accessGroupName} AccessGroup not found.");
		}

		//get userId from accessgroup
		var userDto = await _userRepository.GetAsync(userId);
		var supremaUserId = accessGroup.users.Where(x => x.name.ToUpper() == userDto.UserName.ToUpper())
		                               .Select(x => x.user_id).FirstOrDefault();

		//find card linked with userId
		var cardCollectionResponse = await GetAllCards();
		var cardDto = cardCollectionResponse.CardCollection.rows
		                                    .Where(x => x.user_id != null &&
		                                                x.user_id.user_id.ToUpper() == supremaUserId).FirstOrDefault();
		if (cardDto != null)
		{
			qrcode = cardDto.card_id;
		}

		//create qrcode base64
		var _qrCode = new QRCodeGenerator();
		var _qrCodeData = _qrCode.CreateQrCode(qrcode, QRCodeGenerator.ECCLevel.Q);
		var qrCodeObj = new QRCode(_qrCodeData);
		var qrCodeImage = qrCodeObj.GetGraphic(20);
		using (var stream = new MemoryStream())
		{
			qrCodeImage.Save(stream, ImageFormat.Png);
			var qrBytes = stream.ToArray();
			var accessCodeBase64 = Convert.ToBase64String(qrBytes);
			return new QrCodeDto
			{
				QrCode = accessCodeBase64
			};
		}
	}

	[AllowAnonymous]
	public virtual async Task DeleteExpiredCardAndUsers()
	{
		var adminPrincipal = new ClaimsPrincipal(
			new ClaimsIdentity(
				new Claim[]
				{
					//new Claim(AbpClaimTypes.UserId, Guid.NewGuid().ToString()),
					new Claim(AbpClaimTypes.UserName, "admin"),
					new Claim(AbpClaimTypes.Role, "admin"),
				}
			)
		);

		using (_currentPrincipalAccessor.Change(adminPrincipal))
		{
			Logger.LogInformation("Starting Delete Expired Cards And Users...");
			var userCollectionResponse = await GetUsers();
			var expiredUsers = userCollectionResponse.UserCollection.rows.Where(x =>
				x.expired == "true").ToList();
			var expiredUserIds = expiredUsers.Select(x => x.user_id).ToList();

			var cardCollectionResponse = await GetAllCards();
			var expiredCards = cardCollectionResponse.CardCollection.rows
			                                         .Where(x => x.user_id != null &&
			                                                     expiredUserIds.Contains(x.user_id.user_id)).ToList();

			Logger.LogInformation($"Found {expiredUsers.Count} expired Users!");
			Logger.LogInformation($"Found {expiredCards.Count} expired Cards!");

			foreach (var expiredUserDto in expiredUsers)
			{
				try
				{
					Logger.LogInformation($"Deleting {expiredUserDto.name}");
					await DeleteUser(expiredUserDto.user_id);
				}
				catch (Exception e)
				{
					Logger.LogException(e);
				}
			}

			foreach (var expiredCard in expiredCards)
			{
				try
				{
					await DeleteCard(expiredCard.card_id);
				}
				catch (Exception e)
				{
					Logger.LogException(e);
				}
			}

			Logger.LogInformation("Completed Delete Expired Cards And Users...");
		}
	}

	#endregion


	public virtual async Task<EventLogQueryResponse> GetDoorEventLogsByDeviceIds(List<string> deviceIds)
	{
		var eventLogQueryRequest = new EventLogQueryRequest
		{
			Query = new Query
			{
				limit = 20,
				conditions = new List<Condition>()
				{
					new Condition
					{
						column = "device_id.id",
						@operator = 2,
						values = deviceIds
					},
					new Condition
					{
						column = "event_type_id",
						@operator = 0,
						values = new List<string>() { "4102" }
					},

					// new Condition
					// {
					// 	column = "datetime",
					// 	@operator = 0,
					// 	values = new List<string>()
					// 		{ DateTime.Now.AddHours(-1).ToString("s") + "Z", DateTime.Now.ToString("s") + "Z" }
					// },
				},
				orders = new List<Order>()
				{
					new Order
					{
						column = "datetime",
						descending = true
					}
				}
			}
		};
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/events/search");
		request.AddHeader("bs-session-id", _bsSessionId);
		var payload = JsonConvert.SerializeObject(eventLogQueryRequest,Newtonsoft.Json.Formatting.None, 
			new JsonSerializerSettings { 
				NullValueHandling = NullValueHandling.Ignore
			});
		request.AddJsonBody(payload);

		var response = await _restApiHelper.PostAsync(request);
		var eventLogQueryResponse = JsonConvert.DeserializeObject<EventLogQueryResponse>(response);
		return eventLogQueryResponse;
	}

	public virtual async Task<DeviceCollectionResponse> GetDevices()
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/devices");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.GetAsync(request);
		var deviceCollectionResponse = JsonConvert.DeserializeObject<DeviceCollectionResponse>(response.Content);
		return deviceCollectionResponse;
	}

	public virtual async Task<DeviceResponse> GetDevice(string deviceId)
	{
		await Login();
		var request = new RestRequest($"{_apiEndpoint}/api/devices/{deviceId}");
		request.AddHeader("bs-session-id", _bsSessionId);

		var response = await _restApiHelper.GetAsync(request);
		var deviceResponse = JsonConvert.DeserializeObject<DeviceResponse>(response.Content);
		return deviceResponse;
	}
}