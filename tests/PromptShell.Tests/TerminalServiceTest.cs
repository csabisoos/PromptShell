namespace PromptShell.Tests;
using PromptShell.Services;
using Xunit;

public class TerminalServiceTest
{
    [Fact]
    public async Task ExecuteCommandAsync_WithSimpleEcho_ShouldReturnCorrectOutput()
    {
        // Arrange
        ITerminalService terminalService = new TerminalService();
        string testCommand = "echo 'Hello PromptShell'";

        // Act
        var result = await terminalService.ExecuteCommandAsync(testCommand);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal("Hello PromptShell", result.Output);
        Assert.Empty(result.Error);
    }
}