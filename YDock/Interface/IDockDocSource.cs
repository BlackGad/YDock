namespace YDock.Interface
{
    public interface IDockDocSource : IDockSource
    {
        #region Properties

        /// <summary>
        ///     源文件的名称
        /// </summary>
        string FileName { get; }

        /// <summary>
        ///     源文件的完整路径
        /// </summary>
        string FullFileName { get; }

        /// <summary>
        ///     源文件是否修改
        /// </summary>
        bool IsModified { get; set; }

        #endregion

        #region Members

        /// <summary>
        ///     提供一个操作，在关闭选项卡前会询问是否允许关闭
        /// </summary>
        /// <returns></returns>
        bool AllowClose();

        /// <summary>
        ///     重新加载源文件
        /// </summary>
        void ReLoad();

        /// <summary>
        ///     源文件的保存接口
        /// </summary>
        void Save();

        #endregion
    }
}