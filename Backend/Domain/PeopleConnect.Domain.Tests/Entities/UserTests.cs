using PeopleConnect.Domain.Entities;
using PeopleConnect.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace PeopleConnect.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUser()
    {
        
        var username = "testuser";
        var passwordHash = "hashedpassword";
        var role = UserRole.User;

        
        var user = new User(username, passwordHash, role);

        
        user.Id.Should().NotBeEmpty();
        user.Username.Should().Be(username);
        user.PasswordHash.Should().Be(passwordHash);
        user.Role.Should().Be(role);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidUsername_ShouldThrowUserException(string invalidUsername)
    {
        
        var passwordHash = "hashedpassword";

        
        var exception = Assert.Throws<UserException>(() => new User(invalidUsername, passwordHash));
        exception.Code.Should().Be("USER_INVALID_USERNAME");
        exception.Message.Should().Contain("Username é obrigatório");
    }

    [Fact]
    public void Constructor_WithUsernameTooShort_ShouldThrowUserException()
    {
        
        var username = "ab"; 
        var passwordHash = "hashedpassword";

        
        var exception = Assert.Throws<UserException>(() => new User(username, passwordHash));
        exception.Code.Should().Be("USER_INVALID_USERNAME");
        exception.Message.Should().Contain("pelo menos 3 caracteres");
    }

    [Fact]
    public void Constructor_WithUsernameTooLong_ShouldThrowUserException()
    {
        
        var username = new string('a', 51); 
        var passwordHash = "hashedpassword";

        
        var exception = Assert.Throws<UserException>(() => new User(username, passwordHash));
        exception.Code.Should().Be("USER_INVALID_USERNAME");
        exception.Message.Should().Contain("no máximo 50 caracteres");
    }

    [Theory]
    [InlineData("user@invalid")]
    [InlineData("user space")]
    [InlineData("user#invalid")]
    public void Constructor_WithInvalidUsernameFormat_ShouldThrowUserException(string invalidUsername)
    {
        
        var passwordHash = "hashedpassword";

        
        var exception = Assert.Throws<UserException>(() => new User(invalidUsername, passwordHash));
        exception.Code.Should().Be("USER_INVALID_USERNAME");
        exception.Message.Should().Contain("só pode conter letras, números, pontos, hífens e underscores");
    }

    [Theory]
    [InlineData("user123")]
    [InlineData("user.name")]
    [InlineData("user-name")]
    [InlineData("user_name")]
    public void Constructor_WithValidUsernameFormat_ShouldCreateUser(string validUsername)
    {
        
        var passwordHash = "hashedpassword";

        
        var user = new User(validUsername, passwordHash);

        
        user.Username.Should().Be(validUsername);
    }

    [Fact]
    public void SetPerson_WithValidPerson_ShouldAssociatePerson()
    {
        
        var user = new User("testuser", "hashedpassword");
        var person = new Person("Test User", "11144477735", DateTime.Today.AddYears(-25));

        
        user.SetPerson(person);

        
        user.PersonId.Should().Be(person.Id);
        user.Person.Should().Be(person);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetPerson_WithNullPerson_ShouldThrowArgumentNullException()
    {
        
        var user = new User("testuser", "hashedpassword");

        
        Assert.Throws<ArgumentNullException>(() => user.SetPerson(null!));
    }

    [Fact]
    public void IsAdmin_WhenUserIsAdmin_ShouldReturnTrue()
    {
        
        var user = new User("admin", "hashedpassword", UserRole.Admin);

        
        user.IsAdmin().Should().BeTrue();
    }

    [Fact]
    public void IsAdmin_WhenUserIsNotAdmin_ShouldReturnFalse()
    {
        
        var user = new User("user", "hashedpassword", UserRole.User);

        
        user.IsAdmin().Should().BeFalse();
    }

    [Fact]
    public void CanEditPerson_WhenAdmin_ShouldReturnTrue()
    {
        
        var user = new User("admin", "hashedpassword", UserRole.Admin);
        var personId = Guid.NewGuid();

        
        user.CanEditPerson(personId).Should().BeTrue();
    }

    [Fact]
    public void CanEditPerson_WhenOwner_ShouldReturnTrue()
    {
        
        var user = new User("user", "hashedpassword", UserRole.User);
        var person = new Person("Test User", "11144477735", DateTime.Today.AddYears(-25));
        user.SetPerson(person);

        
        user.CanEditPerson(person.Id).Should().BeTrue();
    }

    [Fact]
    public void CanEditPerson_WhenNotOwnerAndNotAdmin_ShouldReturnFalse()
    {
        
        var user = new User("user", "hashedpassword", UserRole.User);
        var otherPersonId = Guid.NewGuid();

        
        user.CanEditPerson(otherPersonId).Should().BeFalse();
    }

    [Fact]
    public void ValidateCanEditPerson_WhenNotAuthorized_ShouldThrowAuthorizationException()
    {
        
        var user = new User("user", "hashedpassword", UserRole.User);
        var otherPersonId = Guid.NewGuid();

        
        var exception = Assert.Throws<AuthorizationException>(() => user.ValidateCanEditPerson(otherPersonId));
        exception.Code.Should().Be("AUTH_CANNOT_EDIT_OTHERS_PERSON");
    }

    [Fact]
    public void ValidateCanDeleteUser_WhenAdminDeletingSelf_ShouldThrowUserException()
    {
        
        var admin = new User("admin", "hashedpassword", UserRole.Admin);

        
        var exception = Assert.Throws<UserException>(() => admin.ValidateCanDeleteUser(admin.Id));
        exception.Code.Should().Be("USER_CANNOT_DELETE_SELF");
    }

    [Fact]
    public void ValidateCanDeleteUser_WhenUserDeletingOther_ShouldThrowAuthorizationException()
    {
        
        var user = new User("user", "hashedpassword", UserRole.User);
        var otherUserId = Guid.NewGuid();

        
        var exception = Assert.Throws<AuthorizationException>(() => user.ValidateCanDeleteUser(otherUserId));
        exception.Code.Should().Be("AUTH_CANNOT_DELETE_OTHER_USERS");
    }
}
