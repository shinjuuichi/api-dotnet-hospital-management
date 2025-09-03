using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.Patient;
using WebAPI.Models;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;

namespace WebAPI.Services.Implements;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Patient> _patientRepository;

    public PatientService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _patientRepository = _unitOfWork.Repository<Patient>();
    }

    public async Task<List<PatientListResponseDto>> GetAllPatientsAsync()
    {
        var patients = await _patientRepository
            .GetAllQueryable(new[] { "User" })
            .Select(p => new PatientListResponseDto
            {
                Id = p.Id,
                FullName = p.User.FullName,
                Email = p.User.Email,
                PhoneNumber = p.User.PhoneNumber,
                Avatar = p.User.Avatar,
                Address = p.Address,
                InsuranceNo = p.InsuranceNo
            })
            .ToListAsync();

        return patients;
    }

    public async Task<PatientResponseDto> GetPatientByIdAsync(int id)
    {
        var patient = await _patientRepository
            .GetAllQueryable(new[] { "User" })
            .Where(p => p.Id == id)
            .Select(p => new PatientResponseDto
            {
                Id = p.Id,
                FullName = p.User.FullName,
                Email = p.User.Email,
                PhoneNumber = p.User.PhoneNumber,
                DateOfBirth = p.User.DateOfBirth,
                Gender = (int)p.User.Gender,
                Avatar = p.User.Avatar,
                Address = p.Address,
                InsuranceNo = p.InsuranceNo,
                CreationDate = p.CreationDate ?? DateTime.UtcNow
            })
            .FirstOrDefaultAsync();

        if (patient == null)
            throw new InvalidOperationException("Patient not found");

        return patient;
    }
}
