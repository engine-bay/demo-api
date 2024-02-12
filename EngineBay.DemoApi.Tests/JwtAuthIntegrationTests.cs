namespace EngineBay.DemoApi.Tests
{
    using System.Net;
    using Alba;
    using EngineBay.Auditing;
    using EngineBay.Authentication;
    using EngineBay.Core;
    using EngineBay.DemoModule;
    using EngineBay.Persistence;
    using Xunit;

    [Collection(JwtCollection.JWTCOLLECTION)]
    public class JwtAuthIntegrationTests
    {
        private readonly IAlbaHost host;
        private readonly string sampleUserAuth;
        private readonly string adminUserAuth = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWRtaW4ifQ._rmu_6xQ4fJ2WqPp_S9nzzfrBkENwLY8X8BJeqvx78s";

        public JwtAuthIntegrationTests(JwtAuthFixture fixture)
        {
            ArgumentNullException.ThrowIfNull(fixture);
            ArgumentNullException.ThrowIfNull(fixture.AlbaHost);

            this.host = fixture.AlbaHost;
            this.sampleUserAuth = fixture.SampleUserAuthHeader;
        }

        [Fact]
        public async Task CanGetSwagger()
        {
            await this.host.Scenario(scenario =>
            {
                scenario.Get.Url("/swagger/index.html");
                scenario.StatusCodeShouldBeOk();
                scenario.ContentShouldContain("Swagger");
            });
        }

        [Fact]
        public async Task CanGetAuditRecords()
        {
            var result = await this.host.Scenario(scenario =>
            {
                scenario.Get.Url("/api/v1/audit-entries");
                scenario.StatusCodeShouldBeOk();
            });

            var output = await result.ReadAsJsonAsync<PaginatedDto<AuditEntryDto>>();
            Assert.NotNull(output);
        }

        [Theory]
        [InlineData("abc", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWJjIn0.lGHPhzraFFSxMfUI0UNVG91HObr28ehjdqOuUwtTOS0")]
        [InlineData("bobby", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYm9iYnkifQ.zLQ4Q3fEMO3DCuDp0rMZtdANBop-4a-HrPLDrkxlYGw")]
        public async Task CreatingUserCanLoginAndWillBeAudited(string username, string authHeader)
        {
            var user = new CreateUserDto(username);

            var registerResult = await this.host.Scenario(scenario =>
            {
                scenario.Post
                .Json(user)
                .ToUrl("/api/v1/register");
                scenario.StatusCodeShouldBeOk();
            });

            var registerOutput = await registerResult.ReadAsJsonAsync<ApplicationUserDto>();
            Assert.NotNull(registerOutput);
            Assert.True(registerOutput.Id > Guid.Empty);
            Assert.Equal(username, registerOutput.Username);

            var userInfoResult = await this.host.Scenario(scenario =>
            {
                scenario.WithRequestHeader("Authorization", authHeader);
                scenario.Get.Url("/api/v1/userInfo");
                scenario.StatusCodeShouldBeOk();
            });

            var userInfoOutput = await userInfoResult.ReadAsJsonAsync<ApplicationUserDto>();
            Assert.NotNull(userInfoOutput);
            Assert.Equal(registerOutput.Id, userInfoOutput.Id);
            Assert.Equal(registerOutput.Username, userInfoOutput.Username);

            var auditResult = await this.host.Scenario(scenario =>
            {
                scenario.Get.Url("/api/v1/audit-entries?limit=100");
                scenario.StatusCodeShouldBeOk();
            });

            var auditOutput = await auditResult.ReadAsJsonAsync<PaginatedDto<AuditEntryDto>>();
            Assert.NotNull(auditOutput);
            Assert.True(auditOutput.Total >= 1);
            Assert.Equal(1, auditOutput.Data.Count((entry) =>
            {
                return entry.ActionType == DatabaseOperationConstants.INSERT &&
                entry.EntityName == nameof(ApplicationUser) &&
                Guid.Parse(entry.EntityId) == userInfoOutput.Id;
            }));
        }

        [Fact]
        public async Task AdminUserWasSeeded()
        {
            var userInfoResult = await this.host.Scenario(scenario =>
            {
                scenario.WithRequestHeader("Authorization", this.adminUserAuth);
                scenario.Get.Url("/api/v1/userInfo");
                scenario.StatusCodeShouldBeOk();
            });

            var userInfoOutput = await userInfoResult.ReadAsJsonAsync<ApplicationUserDto>();
            Assert.NotNull(userInfoOutput);
            Assert.True(userInfoOutput.Id > Guid.Empty);
            Assert.Equal("admin", userInfoOutput.Username);
        }

        [Fact]
        public async Task PermissionEndpointsProtectedFromUnauthenticated()
        {
            await this.host.Scenario(scenario =>
            {
                scenario.Get.Url("/api/v1/permissions");
                scenario.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
            });

            await this.host.Scenario(scenario =>
            {
                scenario.Get.Url("/api/v1/groups");
                scenario.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
            });

            await this.host.Scenario(scenario =>
            {
                scenario.Get.Url("/api/v1/roles");
                scenario.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
            });
        }

        [Fact]
        public async Task PermissionEndpointsProtectedFromUnauthorized()
        {
            await this.host.Scenario(scenario =>
            {
                scenario.WithRequestHeader("Authorization", this.sampleUserAuth);
                scenario.Get.Url("/api/v1/permissions");
                scenario.StatusCodeShouldBe(HttpStatusCode.Forbidden);
            });

            await this.host.Scenario(scenario =>
            {
                scenario.WithRequestHeader("Authorization", this.sampleUserAuth);
                scenario.Get.Url("/api/v1/groups");
                scenario.StatusCodeShouldBe(HttpStatusCode.Forbidden);
            });

            await this.host.Scenario(scenario =>
            {
                scenario.WithRequestHeader("Authorization", this.sampleUserAuth);
                scenario.Get.Url("/api/v1/roles");
                scenario.StatusCodeShouldBe(HttpStatusCode.Forbidden);
            });
        }

        [Fact]
        public async Task AdminCanAccessPermissionEndpoints()
        {
            await this.host.Scenario(scenario =>
            {
                scenario.WithRequestHeader("Authorization", this.adminUserAuth);
                scenario.Get.Url("/api/v1/permissions");
                scenario.StatusCodeShouldBeOk();
            });

            await this.host.Scenario(scenario =>
            {
                scenario.WithRequestHeader("Authorization", this.adminUserAuth);
                scenario.Get.Url("/api/v1/groups");
                scenario.StatusCodeShouldBeOk();
            });

            await this.host.Scenario(scenario =>
            {
                scenario.WithRequestHeader("Authorization", this.adminUserAuth);
                scenario.Get.Url("/api/v1/roles");
                scenario.StatusCodeShouldBeOk();
            });
        }

        [Fact]
        public async Task SimpleUserCanInteractWithTodo()
        {
            var createListDto = new CreateOrUpdateTodoListDto("example list")
            {
                Description = "example description",
            };

            var listResult = await this.host.Scenario(scenario =>
            {
                scenario.WithRequestHeader("Authorization", this.sampleUserAuth);
                scenario.Post.Json(createListDto).ToUrl("/api/v1/lists");
                scenario.StatusCodeShouldBe(HttpStatusCode.Created);
            });

            var listOutput = await listResult.ReadAsJsonAsync<TodoListDto>();
            Assert.NotNull(listOutput);
            Assert.Equal(createListDto.Name, listOutput.Name);
            Assert.Equal(createListDto.Description, listOutput.Description);
            Assert.NotEqual(Guid.Empty, listOutput.Id);

            var createItemDto = new CreateTodoItemDto("example item", listOutput.Id)
            {
                Description = "example item description",
                DueDate = new DateTime(2025, 1, 1),
            };

            var itemResult = await this.host.Scenario(scenario =>
            {
                scenario.WithRequestHeader("Authorization", this.sampleUserAuth);
                scenario.Post.Json(createItemDto).ToUrl("/api/v1/lists/items");
                scenario.StatusCodeShouldBe(HttpStatusCode.Created);
            });

            var itemOutput = await itemResult.ReadAsJsonAsync<TodoItemDto>();
            Assert.NotNull(itemOutput);
            Assert.NotEqual(Guid.Empty, itemOutput.Id);
            Assert.Equal(createItemDto.Name, itemOutput.Name);
            Assert.Equal(createItemDto.ListId, itemOutput.ListId);
            Assert.Equal(createItemDto.Description, itemOutput.Description);
            Assert.Equal(createItemDto.DueDate, itemOutput.DueDate);
            Assert.False(itemOutput.Completed);

            var populatedListResult = await this.host.Scenario(scenario =>
            {
                scenario.WithRequestHeader("Authorization", this.sampleUserAuth);
                scenario.Get.Url("/api/v1/lists/" + listOutput.Id);
                scenario.StatusCodeShouldBeOk();
            });

            var populatedListOutput = await populatedListResult.ReadAsJsonAsync<TodoListDto>();
            Assert.NotNull(populatedListOutput);
            Assert.NotNull(populatedListOutput.TodoItems);
            Assert.Single(populatedListOutput.TodoItems);
            Assert.Equal(itemOutput.Id, populatedListOutput.TodoItems[0].Id);
        }
    }
}
