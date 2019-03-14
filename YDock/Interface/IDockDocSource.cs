namespace YDock.Interface
{
    public interface IDockDocSource : IDockSource
    {
        #region Properties

        /// <summary>
        ///     The name of the source file
        /// </summary>
        string FileName { get; }

        /// <summary>
        ///     The full path to the source file
        /// </summary>
        string FullFileName { get; }

        /// <summary>
        ///     Is the source file is modified
        /// </summary>
        bool IsModified { get; set; }

        #endregion

        #region Members

        /// <summary>
        ///     Provide an action that asks if it is allowed to close before closing the tab
        /// </summary>
        /// <returns></returns>
        bool AllowClose();

        /// <summary>
        ///     Reload source file
        /// </summary>
        void Reload();

        /// <summary>
        ///     Source file save interface
        /// </summary>
        void Save();

        #endregion
    }
}