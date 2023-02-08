using ChatShared.Json;
using ChatShared.Types;
using FluentValidation;

namespace ChatServer.Handlers
{
    //Todo: Move this else where
    public class LoginRegisterEventValidator : AbstractValidator<LoginRegisterEvent>
	{
		public LoginRegisterEventValidator(EntityFramework context) //DI Container
		{
			RuleFor(x => x.Password)
				.NotEmpty()
				.Length(6, 27)
				.WithMessage(ErrorMessages.InvalidPasswordLength);

			When(x => x.Username == null || x.Email == null, () =>
			{
				RuleFor(x => x.Username).NotEmpty();
				RuleFor(x => x.Email).NotEmpty();
			});

			RuleFor(x => x.Email)
				.EmailAddress()
				.Length(0, 254)
				.Must(email => !context.Users.Any(user => user.Email == email))
				.WithMessage(ErrorMessages.EmailAlreadyExists);
		}
	}
}
