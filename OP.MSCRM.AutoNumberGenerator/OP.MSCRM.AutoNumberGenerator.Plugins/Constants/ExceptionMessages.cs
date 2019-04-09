namespace OP.MSCRM.AutoNumberGenerator.Plugins.Constants
{
    public static class ExceptionMessages
    {
        public const string Name = "An error occurred in the {0} plug-in.";

        public const string CantGenerateMsg = "Can't generate Auto-Number.";

        public const string CantSetMsg = "Can't set Auto-Number value of {0} field in current entity.";

        public const string CantUpdateManuallyMsg = "Automatically generated number of {0} field can't be updated manually.";

        public const string CantUpdateMsg = "Can't update current entity Auto-Number field.";

        public const string ConfigCantFindMsg = "Can't find Auto-Number Configuration entity.";

        public const string ConfigCantUpdateMsg = "Can't update Sequence field of Auto-Number Configuration entity.";
    }
}
