using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Quest;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Quest;

public class QuestTests
{
    private readonly QuestDataDataStore _questDataStore;
    private readonly QuestService _questService;

    public QuestTests()
    {
        _questDataStore = new QuestDataDataStore();
        _questService = new QuestService(_questDataStore);
    }

    [Fact]
    public void GetQuest_When_Quest_Exists_Returns_Quest()
    {
        // Arrange
        var quest = CreateTestQuest(1, "Test Quest", 0);
        _questDataStore.AddOrUpdate(1, quest);

        // Act
        var result = _questService.GetQuest(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test Quest");
    }

    [Fact]
    public void GetQuest_When_Quest_Does_Not_Exist_Returns_Null()
    {
        // Act
        var result = _questService.GetQuest(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Started_quest_returns_true()
    {
        // Arrange
        var quest = CreateTestQuest(1, "Test Quest", 0);
        _questDataStore.AddOrUpdate(1, quest);

        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 0 } // Quest started with value 0
        });

        // Act
        var result = _questService.QuestIsStarted(player, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Started_quest_with_higher_value_returns_true()
    {
        // Arrange
        var quest = CreateTestQuest(1, "Test Quest", 0);
        _questDataStore.AddOrUpdate(1, quest);

        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 5 } // Quest started with value 5
        });

        // Act
        var result = _questService.QuestIsStarted(player, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Not_started_quest_returns_false()
    {
        // Arrange
        var quest = CreateTestQuest(1, "Test Quest", 0);
        _questDataStore.AddOrUpdate(1, quest);

        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, -1 } // Quest not started
        });

        // Act
        var result = _questService.QuestIsStarted(player, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Quest_not_started_when_quest_does_not_exist()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();

        // Act
        var result = _questService.QuestIsStarted(player, 999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Completed_quest_with_no_missions_returns_true()
    {
        // Arrange
        var quest = CreateTestQuest(1, "Test Quest", 0);
        _questDataStore.AddOrUpdate(1, quest);

        var player = PlayerTestDataBuilder.Build();

        // Act
        var result = _questService.QuestIsCompleted(player, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Completed_quest_with_all_missions_completed_returns_true()
    {
        // Arrange
        var quest = CreateTestQuest(1, "Test Quest", 0);
        quest.Missions =
        [
            CreateTestMission(1, "Mission 1", 0, 1),
            CreateTestMission(2, "Mission 2", 0, 1)
        ];
        _questDataStore.AddOrUpdate(1, quest);

        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 1 }, // Mission 1 completed
            { 2, 1 } // Mission 2 completed
        });

        // Act
        var result = _questService.QuestIsCompleted(player, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Incomplete_quest_with_some_missions_not_completed_returns_false()
    {
        // Arrange
        var quest = CreateTestQuest(1, "Test Quest", 0);
        quest.Missions =
        [
            CreateTestMission(1, "Mission 1", 0, 1),
            CreateTestMission(2, "Mission 2", 0, 1)
        ];
        _questDataStore.AddOrUpdate(1, quest);

        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 1 }, // Mission 1 completed
            { 2, 0 } // Mission 2 not completed
        });

        // Act
        var result = _questService.QuestIsCompleted(player, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Quest_not_completed_when_quest_does_not_exist()
    {
        // Arrange
        var player = PlayerTestDataBuilder.Build();

        // Act
        var result = _questService.QuestIsCompleted(player, 999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Mission_completed_when_value_equals_end_value()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 1);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 1 } // Mission completed
        });

        // Act
        var result = _questService.MissionIsCompleted(player, mission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Mission_not_completed_when_value_less_than_end_value()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 1);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 0 } // Mission not completed
        });

        // Act
        var result = _questService.MissionIsCompleted(player, mission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Mission_not_completed_when_value_greater_than_end_value()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 1);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 2 } // Mission value exceeds end value
        });

        // Act
        var result = _questService.MissionIsCompleted(player, mission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Mission_completed_with_ignore_end_value_when_value_greater_than_or_equal_to_end_value()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 1, true);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 1 } // Mission completed
        });

        // Act
        var result = _questService.MissionIsCompleted(player, mission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Mission_completed_with_ignore_end_value_when_value_greater_than_end_value()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 1, true);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 5 } // Mission value exceeds end value but ignore end value is true
        });

        // Act
        var result = _questService.MissionIsCompleted(player, mission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Mission_not_completed_when_storage_value_is_minus_one()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 1);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, -1 } // Mission not started
        });

        // Act
        var result = _questService.MissionIsCompleted(player, mission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Mission_not_completed_when_mission_is_null()
    {
        // Arrange
        Mission mission = null;
        var player = PlayerTestDataBuilder.Build();

        // Act
        var result = _questService.MissionIsCompleted(player, mission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Mission_started_when_value_between_start_and_end_values()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 2);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 1 } // Mission started
        });

        // Act
        var result = _questService.MissionIsStarted(player, mission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Mission_started_when_value_equals_start_value()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 2);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 0 } // Mission started
        });

        // Act
        var result = _questService.MissionIsStarted(player, mission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Mission_started_when_value_equals_end_value()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 2);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 2 } // Mission at end value
        });

        // Act
        var result = _questService.MissionIsStarted(player, mission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Mission_not_started_when_value_less_than_start_value()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 1, 2);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 0 } // Value less than start value
        });

        // Act
        var result = _questService.MissionIsStarted(player, mission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Mission_not_started_when_value_greater_than_end_value()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 2);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 3 } // Value greater than end value
        });

        // Act
        var result = _questService.MissionIsStarted(player, mission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Mission_started_with_ignore_end_value_when_value_greater_than_end_value()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 2, true);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 5 } // Value greater than end value but ignore end value is true
        });

        // Act
        var result = _questService.MissionIsStarted(player, mission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Mission_not_started_when_storage_value_is_minus_one()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 2);
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, -1 } // Mission not started
        });

        // Act
        var result = _questService.MissionIsStarted(player, mission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Mission_not_started_when_mission_is_null()
    {
        // Arrange
        Mission mission = null;
        var player = PlayerTestDataBuilder.Build();

        // Act
        var result = _questService.MissionIsStarted(player, mission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Get_mission_description_with_description_returns_formatted_description()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 1);
        mission.Description = "Current state: |STATE|\\nNext step: |STATE|";
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 5 } // Current state value
        });

        // Act
        var result = _questService.GetMissionDescription(player, mission);

        // Assert
        result.Should().Be("Current state: 5\nNext step: 5");
    }

    [Fact]
    public void Get_mission_description_without_description_returns_state_description()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 1);
        mission.Description = null;
        mission.States =
        [
            new MissionState { Id = 0, Description = "Not started" },
            new MissionState { Id = 1, Description = "Completed" }
        ];
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 1 } // Current state value
        });

        // Act
        var result = _questService.GetMissionDescription(player, mission);

        // Assert
        result.Should().Be("Completed");
    }

    [Fact]
    public void Get_mission_description_without_description_and_states_returns_null()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 1);
        mission.Description = null;
        mission.States = null;
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 1 } // Current state value
        });

        // Act
        var result = _questService.GetMissionDescription(player, mission);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Get_mission_description_with_empty_description_returns_state_description()
    {
        // Arrange
        var mission = CreateTestMission(1, "Test Mission", 0, 1);
        mission.Description = "";
        mission.States =
        [
            new MissionState { Id = 0, Description = "Not started" },
            new MissionState { Id = 1, Description = "Completed" }
        ];
        var player = PlayerTestDataBuilder.Build(storages: new Dictionary<uint, int>
        {
            { 1, 1 } // Current state value
        });

        // Act
        var result = _questService.GetMissionDescription(player, mission);

        // Assert
        result.Should().Be("Completed");
    }

    private static Domain.Quest.Quest CreateTestQuest(uint id, string name, uint startValue)
    {
        return new Domain.Quest.Quest
        {
            StartId = id,
            Name = name,
            StartValue = startValue,
            Missions = []
        };
    }

    private static Mission CreateTestMission(uint id, string name, uint startValue, uint endValue,
        bool ignoreEndValue = false)
    {
        return new Mission
        {
            Id = id,
            Name = name,
            StartValue = startValue,
            EndValue = endValue,
            IgnoreEndValue = ignoreEndValue,
            States = []
        };
    }
}