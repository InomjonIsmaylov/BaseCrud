using BaseCrud.Errors.Keys;

namespace BaseCrud.Errors;

public record ValidationServiceError(string ErrorMessage = "Error validating object", string? ErrorKey = null)
    : ServiceError(ErrorMessage, ErrorKey ?? ErrorKeys.Validation.Error);

public record DataTableValidationServiceError(string ErrorMessage = "Error validating metadata of datatable", string? ErrorKey = null)
    : ServiceError(ErrorMessage, ErrorKey ?? ErrorKeys.Validation.Datatable.Error);

public record IdValidationServiceError(string ErrorMessage = "Error validating id", string? ErrorKey = null)
    : ServiceError(ErrorMessage, ErrorKey ?? ErrorKeys.Validation.Id.Error);