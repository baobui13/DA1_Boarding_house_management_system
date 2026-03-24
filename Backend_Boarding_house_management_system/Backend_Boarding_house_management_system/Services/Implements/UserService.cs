using Backend_Boarding_house_management_system.DTOs.User.Requests;
using Backend_Boarding_house_management_system.DTOs.User.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Plainquire.Filter;
using Plainquire.Sort;
using Plainquire.Page;

namespace Backend_Boarding_house_management_system.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
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

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<UserListResponse> GetUsersByFilterAsync(
            EntityFilter<User> filter,
            EntitySort<User> sort,
            EntityPage page)
        {
            var (users, totalCount) = await _userRepository.GetUsersByFilterAsync(filter, sort, page);

            return new UserListResponse
            {
                Items = _mapper.Map<List<UserResponse>>(users),
                TotalCount = totalCount,
                PageNumber = (int)(page.PageNumber ?? 1),
                PageSize = (int)(page.PageSize ?? 10)
            };
        }

        public async Task<bool> UpdateUserAsync(UpdateUserRequest request)
        {
            var user = await _userRepository.GetUserByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Không tìm thấy người dùng với Id '{request.Id}'.");

            _mapper.Map(request, user);

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