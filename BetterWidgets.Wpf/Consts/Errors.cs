namespace BetterWidgets.Consts
{
    public class Errors
    {
        public const string ValueNull = "The value cannot be null or empty.";
        public const string NullReference = "An object reference does not point to an instance of an object.";
        public const string IdNullOrEmpty = "The id parameter is null or empty.";
        public const string ApiKeyIsNullOrEmpty = "The API key is null or empty.";
        public const string JwtTokenIsEmpty = "JWT token is empty.";
        public const string TheTagWasNull = "The was was null or empty";
        public const string CannotAccessAppShell = "Cannot access application shell.";
        public const string CannotRegisterTrayIcon = "Cannot register tray icon.";
        public const string ShareNotSupported = "The sharing feature is not supported on current device.";
        public const string ShareFormatNotSupported = "No such data sharing format supported.";

        public const string NetworkUnavailable = "The network is unavailable. Please check your connection.";

        public const string UnexpectedValueType = "The value has unexpected type {0}";

        public const string SettingsStorageLoadFailed = "Settings storage loading failed.";
        public const string SettingKeyIsEmpty = "The setting key is null or empty. Key: {0}";

        public const string ExtraDataHasNotWidget = "The extra data doesn't have widget object.";

        public const string TypeWasNull = "The target type was null.";
        public const string PageNotRegisteredOrUndefined = "The required page is not registered or not defined";
        public const string WidgetUnclosable = "You cannot close CoreWidget, use Hide() instead or UnpinWidget() is more recomended.";
        public const string WidgetWithSpecifiedIdIsNotExists = "The widget with specified id is not exists.";
        public const string WidgetTypeWasNotRegistered = "The widget type was not registered.";

        public const string PermissionWasNull = "Permission object reference does not point to an instance of an object.";
        public const string WidgetHasNotDefinedPermission = "Widget has not defined persmission {0}.";
        public const string WidgetHasNotAllowedPermission = "Widget has not allowed permission.";
        public const string WidgetIconUriIsNotWellFormed = "The widget icon source uri is not well formed.";
        public const string WidgetMetadataCannotBeNull = "Widget metadata cannot be null.";
        public const string CannotActivateWidgetInstance = "Cannot activate widget instance.";

        public const string SettingsServiceNotLoaded = "The settings service is not loaded.";
        public const string LockTimerIsNotLoaded = "Lock timer is not loaded.";

        public const string ExecutionStateIsUnknown = "Widget execution state is unknown.";

        public const string UnitsModeIsNullOrEmpty = "Weather units mode cannot be null or empty.";
        public const string UnitsModeHasInvalidValue = "Invalid weather units mode value.";
        public const string PlaceNameIsNullOrEmpty = "The place name is null or empty";
        public const string GeocoordinatesIsNotValid = "The geocoordinates is not valid";

        public const string FileNameIsNullOrEmpty = "The file name is null or empty.";

        public const string FileNotFound = "The file with specified name not found.";
        public const string FolderNotFound = "The folder with specified name not found.";
        public const string FileContentIsEmpty = "The file {0} has empty content.";
        public const string DataIsNull = "Cannot write data, because it's null.";

        public const string ArgumentHasInvalidType = "The argument has invalid type.";
        public const string ArgumentHasInvalidFormat = "The argument has invalid format.";
        public const string MSGraphServiceIsNotRegistered = "MS Graph service is not registered.";
        public const string UserIsNotSignedIn = "User is not signed in.";
        public const string MSGraphClientIsNotInitialized = "The MS Graph client is not initialized.";

        public const string CannotOpenDialogWhenAnotherIsOpen = "You're can't open one more dialog when the some dialog is already opened.";
    
        public const string SerializationNull = "The serialization result is null.";
        public const string CannotPinUnregisteredWidget = "Cannot to pin or unpin unregistered widget. Pinnable widgets should be registered with unique id.";
        public static string MediaPlayerIsNotInitialized = "Media player is not initialized.";

        public static string ViewWithIdAlreadyExists = "The view with the same ID is already exists";
        public static string CollectionWasNull = "The items collection was null.";
        public const string EntityGuidCannotBeDefault = "The entity Guid cannot be default parameter: {0}.";
        public const string StoreContextIsNotInitialized = "Store context is not initialized.";

        public const string UIControlIsNotLoaded = "The UI control is not initialized or loaded.";
    }
}
