using AutoMapper;
using WebAPI.DTOs;
using WebAPI.Models;

namespace WebAPI.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
        CreateMap<UpdateProfileDto, User>();

        CreateMap<Specialty, SpecialtyDto>();
        CreateMap<CreateSpecialtyDto, Specialty>();
        CreateMap<UpdateSpecialtyDto, Specialty>();

        CreateMap<Doctor, DoctorDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.SpecialtyName, opt => opt.MapFrom(src => src.Specialty.Name));

        CreateMap<Doctor, DoctorDetailsDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

        CreateMap<Patient, PatientDetailsDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

        CreateMap<Appointment, AppointmentDto>()
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.User.FullName))
            .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.User.FullName : null))
            .ForMember(dest => dest.SpecialtyName, opt => opt.MapFrom(src => src.Specialty.Name));

        CreateMap<Appointment, AppointmentDetailsDto>();
        CreateMap<RequestAppointmentDto, Appointment>();

        CreateMap<Medicine, MedicineDto>();
        CreateMap<CreateMedicineDto, Medicine>();
        CreateMap<UpdateMedicineDto, Medicine>();

        CreateMap<Prescription, PrescriptionDto>()
            .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.User.FullName))
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Appointment.Patient.User.FullName))
            .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.Appointment.PatientId));

        CreateMap<CreatePrescriptionDto, Prescription>();

        CreateMap<PrescriptionDetail, PrescriptionDetailDto>()
            .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine.Name));

        CreateMap<CreatePrescriptionDetailDto, PrescriptionDetail>();
    }
}
