using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetMedicalConditionTypes;

public sealed record GetMedicalConditionTypesQuery(MedicalConditionCategory? Category = null)
    : IRequest<Result<IReadOnlyList<MedicalConditionTypeDto>>>;