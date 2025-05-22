using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bonzer.Propman.App.VisitorInvites;

namespace Bonzer.Propman.App.Suprema;

public interface ISupremaAppService
{
	Task<ResponseJsonDto> AssignCardToUser(string userId, string cardId);

	Task<ResponseJsonDto> CreateAccessGroup();

	Task<CardCollectionResponse> CreateCard(string qrcode);

	Task<UserCollectionResponse> CreateUser(UserCreateUpdateDto userCreateUpdateDto);

	Task<ResponseJsonDto> CreateUserGroup();

	Task<ResponseJsonDto> DeleteAccessGroup(string accessGroupId);

	Task<ResponseJsonDto> DeleteCard(string cardId);

	Task<ResponseJsonDto> DeleteUser(string userId);

	Task<ResponseJsonDto> DeleteUserGroup(string userGroupId);

	Task<ResponseJsonDto> EnrollVisitor(VisitorInviteDto visitorInviteDto);

	Task<ResponseJsonDto> UpdateVisitor(VisitorInviteDto visitorInviteDto);

	Task<ResponseJsonDto> DeleteUserAndCardByQrCode(string accessQrCode);


	Task<AccessGroupCollectionResponse> GetAccessGroups();

	Task<CardCollectionResponse> GetAllCards();

	Task<CardCollectionResponse> GetAssignedCards();

	Task<CardCollectionResponse> GetCardDetail(string cardId);

	Task<CardTypeCollectionResponse> GetCardTypes();

	Task<CardCollectionResponse> GetUnassignedCards();

	Task<UserGroupCollectionResponse> GetUserGroups();

	Task<List<UserGroupDto>> GetUserGroupsByUnitName(string blkName, string unitName);

	Task<UserCollectionResponse> GetUsers();

	Task<List<AccessGroupDto>> GetVisitorAccessGroups(string blkName, string level);

	Task<LoginResponse> Login();

	Task CreateFacilityCardForUser(Guid facilityTypeId, Guid userId, DateTime startTime,
	                               DateTime endTime, string qrcode);

	Task<QrCodeDto> GetFacilityCardForUser(Guid facilityTypeId, Guid userId);
	Task DeleteExpiredCardAndUsers();
	Task<EventLogQueryResponse> GetDoorEventLogsByDeviceIds(List<string> deviceIds);
	Task<DeviceCollectionResponse> GetDevices();
	Task RemoveVisitorFromAccessGroup(string userId, string accessGroupId);
	Task<AccessGroupResponse> GetAccessGroup(string accessGroupId);
	Task<string> UpdateAccessGroup(string accessGroupId, AccessGroupUpdateDto accessGroupUpdateDto);
	Task<DeviceResponse> GetDevice(string deviceId);
	Task<UserUpdateResponse> UpdateUser(UserCreateUpdateDto userCreateUpdateDto);
	Task<List<UserUpdateResponse>> AddUserToFacilityAccessGroup(Guid facilityTypeId, Guid propUnitId);
	Task<List<UserUpdateResponse>> RemoveUserFromFacilityAccessGroup(Guid facilityTypeId, Guid propUnitId);
}