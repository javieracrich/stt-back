using AutoMapper;
using Domain;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<User, UserModel>()
                .ForMember(user => user.Phone, map => map.MapFrom(vm => vm.PhoneNumber))
                .ForMember(user => user.Grade, map => map.MapFrom(vm => vm.Room.Grade))
                .ForMember(user => user.GradeRoomName, map => map.MapFrom(vm => vm.Room.Name))
                .ForMember(user => user.LastStatusId, map => map.Ignore())
                .ForMember(user => user.LastStatusDateTime, map => map.Ignore());

            CreateMap<User, BasicUserModel>();

            CreateMap<NewsItem, NewsItemModel>()
                .ForMember(x => x.AuthorFullName, map => map.MapFrom(x => x.Author.FirstName + " " + x.Author.LastName));

            CreateMap<School, BasicSchoolModel>()
                .ForMember(model => model.Name, map => map.MapFrom(school => school.Name))
                .ForMember(model => model.Code, map => map.MapFrom(school => school.Code));

            CreateMap<Room, RoomModel>();

            CreateMap<Bus, BusModel>()
                .ForMember(s => s.Drivers, map => map.MapFrom(vm => vm.DriversAndSupervisors.Where(x => x.Category == UserCategory.BusDriver)))
                .ForMember(s => s.Supervisors, map => map.MapFrom(vm => vm.DriversAndSupervisors.Where(x => x.Category == UserCategory.Supervisor)));

            CreateMap<Bus, BasicBusModel>()
                .ForMember(s => s.DeviceCode, map => map.MapFrom(vm => vm.Device.DeviceCode));

            CreateMap<Device, DeviceModel>();

            CreateMap<Device, BasicDeviceModel>();

            CreateMap<School, SchoolModel>()
               .ForMember(s => s.Directors, map => map.MapFrom<UserDirectorResolver>());

            CreateMap<Card, CardModel>();

            CreateMap<Alert, AlertModel>()
               .ForMember(s => s.UserCategories, map => map.MapFrom(vm => vm.UserCategories
                                                           .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                                           .Select(x => int.Parse(x))
                                                           .Cast<UserCategory>()));
        }
    }

    public class UserDirectorResolver : IValueResolver<School, SchoolModel, IEnumerable<BasicUserModel>>
    {
        public IEnumerable<BasicUserModel> Resolve(School source, SchoolModel destination, IEnumerable<BasicUserModel> members, ResolutionContext context)
        {
            var directors = source.Users.Where(x => x.Category == UserCategory.SchoolDirector).ToList();

            return directors.Select(x => new BasicUserModel()
            {
                Code = x.Code,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Category = UserCategory.SchoolDirector
            });

        }
    }
}

