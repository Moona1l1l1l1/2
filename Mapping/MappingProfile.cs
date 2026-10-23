using AutoMapper;
using UniversityApi.DTO.Courses;
using UniversityApi.DTO.Courses.V2;
using UniversityApi.DTO.Enrollments;
using UniversityApi.DTO.Students;
using UniversityApi.DTO.Teachers;
using UniversityApi.Models;

namespace UniversityApi.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Student
        CreateMap<Student, StudentDto>();
        CreateMap<Student, StudentDetailsDto>()
            .ForMember(d => d.Courses, o => o.MapFrom(s => s.Enrollments));
        CreateMap<Enrollment, StudentCourseDto>()
            .ForMember(d => d.EnrollmentId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course!.Name))
            .ForMember(d => d.CourseCredits, o => o.MapFrom(s => s.Course!.Credits));
        CreateMap<StudentCreateDto, Student>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Enrollments, o => o.Ignore());
        CreateMap<StudentUpdateDto, Student>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Enrollments, o => o.Ignore());

        // Teacher
        CreateMap<Teacher, TeacherDto>();
        CreateMap<TeacherCreateDto, Teacher>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Courses, o => o.Ignore());
        CreateMap<TeacherUpdateDto, Teacher>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Courses, o => o.Ignore());

        // Course
        CreateMap<Course, CourseDto>()
            .ForMember(d => d.TeacherName,
                o => o.MapFrom(s => s.Teacher != null ? s.Teacher.FirstName + " " + s.Teacher.LastName : string.Empty));
        CreateMap<Teacher, CourseTeacherV2Dto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));
        CreateMap<Course, CourseV2Dto>()
            .ForMember(d => d.EnrolledStudentsCount, o => o.Ignore());
        CreateMap<CourseCreateDto, Course>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Teacher, o => o.Ignore())
            .ForMember(d => d.Enrollments, o => o.Ignore());
        CreateMap<CourseUpdateDto, Course>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Teacher, o => o.Ignore())
            .ForMember(d => d.Enrollments, o => o.Ignore());

        // Enrollment
        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(d => d.StudentName,
                o => o.MapFrom(s => s.Student != null ? s.Student.FirstName + " " + s.Student.LastName : string.Empty))
            .ForMember(d => d.CourseName,
                o => o.MapFrom(s => s.Course != null ? s.Course.Name : string.Empty));
        CreateMap<EnrollmentCreateDto, Enrollment>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.EnrollmentDate, o => o.Ignore())
            .ForMember(d => d.Grade, o => o.Ignore())
            .ForMember(d => d.Student, o => o.Ignore())
            .ForMember(d => d.Course, o => o.Ignore());
        CreateMap<EnrollmentUpdateGradeDto, Enrollment>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.StudentId, o => o.Ignore())
            .ForMember(d => d.CourseId, o => o.Ignore())
            .ForMember(d => d.EnrollmentDate, o => o.Ignore())
            .ForMember(d => d.Student, o => o.Ignore())
            .ForMember(d => d.Course, o => o.Ignore());
    }
}
