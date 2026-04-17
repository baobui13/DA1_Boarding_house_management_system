using Backend_Boarding_house_management_system.DTOs.User.Requests;
using Backend_Boarding_house_management_system.DTOs.User.Responses;
using Backend_Boarding_house_management_system.Entities;
using Backend_Boarding_house_management_system.Exceptions;
using Backend_Boarding_house_management_system.Repositories.Interfaces;
using Backend_Boarding_house_management_system.Services.Interfaces;
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
                user = await _userRepository.GetByIdAsync(request.Id);
            else if (!string.IsNullOrEmpty(request.Email))
                user = await _userRepository.GetByEmailAsync(request.Email);
            else
                throw new BadRequestException("Phai cung cap Id hoac Email.");

            if (user == null)
                throw new NotFoundException("Khong tim thay nguoi dung.");

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<UserListResponse> GetUsersByFilterAsync(
            EntityFilter<User> filter,
            EntitySort<User> sort,
            EntityPage page)
        {
            var (users, totalCount) = await _userRepository.GetByFilterAsync(filter, sort, page);

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
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.Id}'.");

            _mapper.Map(request, user);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UpdateUserAvatarAsync(UpdateUserAvatarRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.Id}'.");

            var isSuccess = await _userRepository.UpdateAvatarAsync(request.Id, request.AvatarUrl);
            if (!isSuccess)
                throw new BadRequestException("Cap nhat anh dai dien that bai.");

            return true;
        }

        public async Task<bool> BlockUserAsync(BlockUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.Id}'.");

            var isSuccess = await _userRepository.BlockUserAsync(request.Id, request.IsBlocked);
            if (!isSuccess)
                throw new BadRequestException("Khoa/mo khoa nguoi dung that bai.");

            return true;
        }

        public async Task<bool> DeleteUserAsync(DeleteUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
                throw new NotFoundException($"Khong tim thay nguoi dung voi Id '{request.Id}'.");

            await _userRepository.DeleteAsync(request.Id);
            return true;
        }
    }
}
