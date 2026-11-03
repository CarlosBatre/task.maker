namespace Application.Exceptions;

public class DuplicateTaskTitleException(string title)
    : Exception($"Ya existe una tarea con el título '{title.Trim()}'.");
