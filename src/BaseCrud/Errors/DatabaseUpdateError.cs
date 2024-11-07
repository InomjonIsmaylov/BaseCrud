using BaseCrud.Errors.Keys;

namespace BaseCrud.Errors;

public record DatabaseUpdateError(string ErrorMessage = "Error while updating the entity", string? ErrorKey = null)
    : ServiceError(ErrorMessage, ErrorKey ?? ErrorKeys.Database.UpdateError);

public record DatabaseInsertError(string ErrorMessage = "Error while inserting the entity", string? ErrorKey = null)
    : ServiceError(ErrorMessage, ErrorKey ?? ErrorKeys.Database.InsertError);