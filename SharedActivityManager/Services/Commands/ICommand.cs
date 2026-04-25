namespace SharedActivityManager.Services.Commands
{
    /// <summary>
    /// Interfața Command - definește metodele Execute, Undo și Redo
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Numele comenzii (pentru afișare în UI)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Execută comanda
        /// </summary>
        Task Execute();

        /// <summary>
        /// Anulează comanda (undo)
        /// </summary>
        Task Undo();

        /// <summary>
        /// Reaplică comanda (redo)
        /// </summary>
        Task Redo();
    }
}