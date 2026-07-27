using AutoMapper;
using Fincore.Application.DTO.Logs;
using Fincore.Domain.Models;

namespace Fincore.Application.Mapping
{
    public class LogsMappingProfile : Profile
    {
        public LogsMappingProfile()
        {
            CreateMap<AuditLogDto, AuditLog>();

            CreateMap<AuditLog, AuditLogResponseDto>()
                .ForMember(
                    dest => dest.AuditByName,
                    opt => opt.MapFrom(
                        src => src.AuditByUser.FullName));


            CreateMap<UserActivityLogDto, UserActivityLog>();

            CreateMap<UserActivityLog, UserActivityLogResponseDto>()
                .ForMember(
                    dest => dest.UserName,
                    opt => opt.MapFrom(
                        src => src.User.FullName));


            CreateMap<NotificationLogDto, NotificationLog>();

            CreateMap<NotificationLog, NotificationLogResponseDto>()
                .ForMember(
                    dest => dest.UserName,
                    opt => opt.MapFrom(
                        src => src.User.FullName));


            CreateMap<ApprovalLogDto, ApprovalLog>();

            CreateMap<ApprovalLog, ApprovalLogResponseDto>()
                .ForMember(
                    dest => dest.ApproverName,
                    opt => opt.MapFrom(
                        src => src.ApproverUser.FullName));
        }
    }
}