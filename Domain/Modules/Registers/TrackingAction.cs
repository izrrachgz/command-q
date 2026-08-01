namespace Domain.Modules.Registers
{
    public enum TrackingAction
    {
        #region For Events

        Entry,
        Exit,

        #endregion

        #region For Places

        Book,
        Cancel,
        Register,
        Visit

        #endregion
    }
}
