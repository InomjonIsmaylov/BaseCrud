using BaseCrud.Errors.Keys;

namespace BaseCrud.Errors;

public record NotFoundServiceError(string ErrorMessage = "Entity not found in database", string? ErrorKey = null)
    : ServiceError(ErrorMessage, ErrorKey ?? ErrorKeys.Database.NotFoundById);