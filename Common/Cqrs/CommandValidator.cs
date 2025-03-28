using FluentValidation;

namespace Common.Cqrs
{
	public abstract class CommandValidator<TCommand> : AbstractValidator<TCommand>
	{
	}
}
