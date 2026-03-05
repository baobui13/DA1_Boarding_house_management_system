using Backend_Boarding_house_management_system.DTOs.User.Requests;
using Backend_Boarding_house_management_system.DTOs.User.Responses;

namespace Backend_Boarding_house_management_system.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse?> GetUserByIdOrEmailAsync(GetUserByIdOrEmailRequest request);

        Task<UserListResponse> GetUsersByFilterAsync(GetUsersByFilterRequest request);

        Task<bool> UpdateUserAsync(UpdateUserRequest request);

        Task<bool> UpdateUserAvatarAsync(UpdateUserAvatarRequest request);

        Task<bool> BlockUserAsync(BlockUserRequest request);

        Task<bool> DeleteUserAsync(DeleteUserRequest request);
    }
}