using Backend_Boarding_house_management_system.DTOs.User.Requests;
using Backend_Boarding_house_management_system.DTOs.User.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserResponse?> GetUserByIdOrEmailAsync(GetUserByIdOrEmailRequest request)
        {
            User? user = null;

            if (!string.IsNullOrEmpty(request.Id))
                user = await _userRepository.GetUserByIdAsync(request.Id);
            else if (!string.IsNullOrEmpty(request.Email))
                user = await _userRepository.GetUserByEmailAsync(request.Email);
            else
                throw new BadRequestException("Phải cung cấp Id hoặc Email.");

            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng.");

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Address = user.Address,
                AvatarUrl = user.AvatarUrl,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task<UserListResponse> GetUsersByFilterAsync(GetUsersByFilterRequest request)
        {
            var (users, totalCount) = await _userRepository.GetUsersByFilterAsync(
                request.Role, 
                request.FullName, 
                request.Address, 
                request.CreatedAfter, 
                request.CreatedBefore,
                request.SortBy ?? "CreatedAt",
                request.IsDescending, 
                request.PageNumber, 
                request.PageSize);

            return new UserListResponse
            {
                Items = users.Select(user => new UserResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    Address = user.Address,
                    AvatarUrl = user.AvatarUrl,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<bool> UpdateUserAsync(UpdateUserRequest request)
        {
            var user = await _userRepository.GetUserByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Không tìm thấy người dùng với Id '{request.Id}'.");

            user.FullName = request.FullName ?? user.FullName;
            user.Address = request.Address ?? user.Address;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

            var isSuccess = await _userRepository.UpdateUserAsync(user);
            if (!isSuccess)
                throw new BadRequestException("Cập nhật thông tin thất bại do lỗi hệ thống.");

            return true;
        }

        public async Task<bool> UpdateUserAvatarAsync(UpdateUserAvatarRequest request)
        {
            var user = await _userRepository.GetUserByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Không tìm thấy người dùng với Id '{request.Id}'.");

            var isSuccess = await _userRepository.UpdateUserAvatarAsync(request.Id, request.AvatarUrl);
            if (!isSuccess)
                throw new BadRequestException("Cập nhật ảnh đại diện thất bại.");

            return true;
        }

        public async Task<bool> BlockUserAsync(BlockUserRequest request)
        {
            var user = await _userRepository.GetUserByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Không tìm thấy người dùng với Id '{request.Id}'.");

            var isSuccess = await _userRepository.BlockUserAsync(request.Id, request.IsBlocked);
            if (!isSuccess)
                throw new BadRequestException("Khóa/Mở khóa người dùng thất bại.");

            return true;
        }

        public async Task<bool> DeleteUserAsync(DeleteUserRequest request)
        {
            var user = await _userRepository.GetUserByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Không tìm thấy người dùng với Id '{request.Id}'.");

            var isSuccess = await _userRepository.DeleteUserAsync(request.Id);
            if (!isSuccess)
                throw new BadRequestException("Xóa người dùng thất bại.");

            return true;
        }
    }
}