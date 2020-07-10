using System.ComponentModel;

namespace Common.Data.Enums
{
    public enum DbErrorType
    {
        [Description("Unknown description")]
        UnknownError = 10,

        [Description("No authorization token present")]
        NoAuthorization = 20,

        [Description("Authorization token has expired")]
        TokenExpired = 30,

        [Description("Authorization failed")]
        AuthorizationFailed = 40,

        [Description("Entity do not exist or you do not have access")]
        ChangesForbidden = 50,

        [Description("Invalid model state")]
        InvalidModel = 60,

        [Description("Action cannot be performed due to db description")]
        DbExceptionError = 80,

        [Description("Null pointer reference")]
        NullPointer = 90,

        [Description("Item already exis")]
        ItemAlreadyExist = 100,

        [Description("Item does not exis")]
        ItemDoesNotExist = 110,

        [Description("Item cannot be changed")]
        ItemCannotBeChanged = 120,

        [Description("Item has childs")]
        ItemHasChilds = 130,

        [Description("Item has relates")]
        ItemHasRelates = 140,

        [Description("Registration was not confirmed")]
        RegistrationNotConfirmed = 150,

        [Description("Same item already exist")]
        Duplicate = 160,

        [Description("You can not do this operation")]
        InvalidOperation = 170
    }
}
