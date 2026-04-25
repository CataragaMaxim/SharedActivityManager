using System.Collections.Concurrent;

namespace SharedActivityManager.Services.Commands
{
    /// <summary>
    /// Invoker - gestionează execuția comenzilor și istoricul Undo/Redo
    /// </summary>
    public class CommandInvoker
    {
        private readonly Stack<ICommand> _undoStack;
        private readonly Stack<ICommand> _redoStack;
        private readonly int _maxHistorySize;

        public event EventHandler<CommandExecutedEventArgs> CommandExecuted;

        public CommandInvoker(int maxHistorySize = 50)
        {
            _undoStack = new Stack<ICommand>();
            _redoStack = new Stack<ICommand>();
            _maxHistorySize = maxHistorySize;
        }

        /// <summary>
        /// Execută o comandă și o adaugă în istoricul Undo
        /// </summary>
        public async Task ExecuteCommand(ICommand command)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Invoker] Executing command: {command.Name}");

                await command.Execute();

                _undoStack.Push(command);
                _redoStack.Clear();

                // Limitează dimensiunea istoricului
                while (_undoStack.Count > _maxHistorySize)
                {
                    _undoStack.Pop();
                }

                CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(command, true));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Invoker] Error executing command: {ex.Message}");
                CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(command, false, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Anulează ultima comandă (Undo)
        /// </summary>
        public async Task<bool> Undo()
        {
            if (_undoStack.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[Invoker] Nothing to undo");
                return false;
            }

            var command = _undoStack.Pop();
            System.Diagnostics.Debug.WriteLine($"[Invoker] Undoing: {command.Name}");

            try
            {
                await command.Undo();
                _redoStack.Push(command);
                CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(command, true, null, true));
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Invoker] Error during undo: {ex.Message}");
                CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(command, false, ex.Message, true));
                return false;
            }
        }

        /// <summary>
        /// Reaplică ultima comandă anulată (Redo)
        /// </summary>
        public async Task<bool> Redo()
        {
            if (_redoStack.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[Invoker] Nothing to redo");
                return false;
            }

            var command = _redoStack.Pop();
            System.Diagnostics.Debug.WriteLine($"[Invoker] Redoing: {command.Name}");

            try
            {
                await command.Redo();
                _undoStack.Push(command);
                CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(command, true, null, false, true));
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Invoker] Error during redo: {ex.Message}");
                CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(command, false, ex.Message, false, true));
                return false;
            }
        }

        /// <summary>
        /// Verifică dacă se poate face Undo
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>
        /// Verifică dacă se poate face Redo
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Obține numele ultimei comenzi pentru Undo
        /// </summary>
        public string GetUndoCommandName()
        {
            return _undoStack.Count > 0 ? _undoStack.Peek().Name : null;
        }

        /// <summary>
        /// Obține numele ultimei comenzi pentru Redo
        /// </summary>
        public string GetRedoCommandName()
        {
            return _redoStack.Count > 0 ? _redoStack.Peek().Name : null;
        }

        /// <summary>
        /// Curăță istoricul comenzilor
        /// </summary>
        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            System.Diagnostics.Debug.WriteLine("[Invoker] History cleared");
        }
    }

    public class CommandExecutedEventArgs : EventArgs
    {
        public ICommand Command { get; }
        public bool Success { get; }
        public string ErrorMessage { get; }
        public bool IsUndo { get; }
        public bool IsRedo { get; }

        public CommandExecutedEventArgs(ICommand command, bool success, string errorMessage = null, bool isUndo = false, bool isRedo = false)
        {
            Command = command;
            Success = success;
            ErrorMessage = errorMessage;
            IsUndo = isUndo;
            IsRedo = isRedo;
        }
    }
}