namespace YDock.Interface
{
    public interface ILayoutSize
    {
        #region Properties

        double DesiredHeight { get; set; }
        double DesiredWidth { get; set; }
        double FloatLeft { get; set; }
        double FloatTop { get; set; }

        #endregion
    }
}