using FluentAssertions;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Domain.Exceptions;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Auth;

public class UserEntityValidationTests
{
    [Fact]
    public void User_WithValidData_ShouldCreateSuccessfully()
    {
        
        var username = "testuser";
        var passwordHash = "hashedPassword";
        var role = UserRole.User;

        
        var user = new User(username, passwordHash, role);

        
        user.Username.Should().Be(username);
        user.PasswordHash.Should().Be(passwordHash);
        user.Role.Should().Be(role);
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void User_WithInvalidUsername_ShouldThrowException()
    {
        
        var invalidUsername = "";
        var passwordHash = "hashedPassword";

        
        var exception = Assert.Throws<UserException>(() => new User(invalidUsername, passwordHash));
        exception.Code.Should().Be("USER_INVALID_USERNAME");
    }

    [Fact]
    public void User_CanEditPerson_WhenAdmin_ShouldReturnTrue()
    {
        
        var admin = new User("admin", "hashedPassword", UserRole.Admin);
        var personId = Guid.NewGuid();

        
        var canEdit = admin.CanEditPerson(personId);

        
        canEdit.Should().BeTrue();
    }

    [Fact]
    public void User_CanEditPerson_WhenOwner_ShouldReturnTrue()
    {
        
        var user = new User("user", "hashedPassword", UserRole.User);
        var person = new Person("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        user.SetPerson(person);

        
        var canEdit = user.CanEditPerson(person.Id);

        
        canEdit.Should().BeTrue();
    }

    [Fact]
    public void User_CanEditPerson_WhenNotOwnerAndNotAdmin_ShouldReturnFalse()
    {
        
        var user = new User("user", "hashedPassword", UserRole.User);
        var otherPersonId = Guid.NewGuid();

        
        var canEdit = user.CanEditPerson(otherPersonId);

        
        canEdit.Should().BeFalse();
    }

    [Fact]
    public void User_ValidateCanDeleteUser_WhenAdminDeletingSelf_ShouldThrowException()
    {
        
        var admin = new User("admin", "hashedPassword", UserRole.Admin);

        
        var exception = Assert.Throws<UserException>(() => admin.ValidateCanDeleteUser(admin.Id));
        exception.Code.Should().Be("USER_CANNOT_DELETE_SELF");
    }

    [Fact]
    public void User_ValidateCanDeleteUser_WhenUserDeletingOther_ShouldThrowException()
    {
        
        var user = new User("user", "hashedPassword", UserRole.User);
        var otherUserId = Guid.NewGuid();

        
        var exception = Assert.Throws<AuthorizationException>(() => user.ValidateCanDeleteUser(otherUserId));
        exception.Code.Should().Be("AUTH_CANNOT_DELETE_OTHER_USERS");
    }
}
