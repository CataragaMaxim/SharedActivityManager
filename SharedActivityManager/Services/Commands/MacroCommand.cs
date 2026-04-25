namespace SharedActivityManager.Services.Commands
{
    /// <summary>
    /// Comandă compusă - grupează mai multe comenzi într-una singură
    /// </summary>
    public class MacroCommand : ICommand
    {
        private readonly List<ICommand> _commands;

        public string Name => $"Macro: {string.Join(", ", _commands.Select(c => c.Name))}";

        public MacroCommand(params ICommand[] commands)
        {
            _commands = commands.ToList();
        }

        public void AddCommand(ICommand command)
        {
            _commands.Add(command);
        }

        public async Task Execute()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Executing Macro with {_commands.Count} commands");

            foreach (var command in _commands)
            {
                await command.Execute();
            }
        }

        public async Task Undo()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Undoing Macro with {_commands.Count} commands");

            // Undo în ordine inversă
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                await _commands[i].Undo();
            }
        }

        public async Task Redo()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Redoing Macro with {_commands.Count} commands");

            foreach (var command in _commands)
            {
                await command.Redo();
            }
        }
    }
}