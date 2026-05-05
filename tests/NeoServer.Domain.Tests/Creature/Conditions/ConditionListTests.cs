using System.Collections;
using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Domain.Tests.Creature.Conditions;

public class ConditionListTests
{
    [Fact]
    public void Add_tracks_conditions_and_supports_cached_enumeration_results()
    {
        var conditionList = new ConditionList();
        var firstCondition = CreateCondition(ConditionType.Burning);
        var secondCondition = CreateCondition(ConditionType.Burning, 0);

        conditionList.Add(firstCondition);

        conditionList.Count.Should().Be(1);

        conditionList.Add(secondCondition);

        conditionList.Count.Should().Be(2);

        var conditionsByType = conditionList.GetByType(ConditionType.Burning);

        conditionsByType.Should().Equal(firstCondition, secondCondition);
        conditionList.GetFirstConditionOfType(ConditionType.Burning).Should().Be(firstCondition);
        conditionList.GetFirstConditionOfType(ConditionType.Burning, out var firstConditionOfType).Should().BeTrue();
        firstConditionOfType.Should().Be(firstCondition);
        conditionList.HasAnyConditionOf(ConditionType.Burning).Should().BeTrue();
        conditionList.HasAnyConditionOf(ConditionType.Burning, out IReadOnlyList<ICondition> conditionsByTypeOut).Should().BeTrue();
        conditionsByTypeOut.Should().Equal(firstCondition, secondCondition);

        var allConditions = conditionList.GetAll();

        allConditions.Should().Equal(firstCondition, secondCondition);
        conditionList.Count.Should().Be(2);
        conditionList.ToList().Should().Equal(firstCondition, secondCondition);
        conditionList.ToList().Should().Equal(firstCondition, secondCondition);
    }

    [Fact]
    public void Missing_condition_types_return_empty_results_and_do_not_throw()
    {
        var conditionList = new ConditionList();

        conditionList.GetByType(ConditionType.Drunk).Should().BeEmpty();
        conditionList.GetFirstConditionOfType(ConditionType.Drunk).Should().BeNull();
        conditionList.GetFirstConditionOfType(ConditionType.Drunk, out var firstCondition).Should().BeFalse();
        firstCondition.Should().BeNull();
        conditionList.HasAnyConditionOf(ConditionType.Drunk).Should().BeFalse();
        conditionList.HasAnyConditionOf(ConditionType.Drunk, out IReadOnlyList<ICondition> conditions).Should().BeFalse();
        conditions.Should().BeEmpty();

        conditionList.Invoking(x => x.Remove(CreateCondition(ConditionType.Drunk))).Should().NotThrow();
        conditionList.Invoking(x => x.RemoveByType(ConditionType.Drunk)).Should().NotThrow();
    }

    [Fact]
    public void Remove_removes_condition_and_invalidates_cached_values()
    {
        var conditionList = new ConditionList();
        var burningCondition = CreateCondition(ConditionType.Burning);
        var drunkCondition = CreateCondition(ConditionType.Drunk);

        conditionList.Add(burningCondition);
        conditionList.Add(drunkCondition);
        conditionList.GetAll();

        conditionList.Remove(burningCondition);

        conditionList.Count.Should().Be(1);
        conditionList.GetByType(ConditionType.Burning).Should().BeEmpty();
        conditionList.GetByType(ConditionType.Drunk).Should().ContainSingle().Which.Should().Be(drunkCondition);
        conditionList.GetAll().Should().ContainSingle().Which.Should().Be(drunkCondition);
    }

    [Fact]
    public void RemoveByType_clears_only_matching_conditions()
    {
        var conditionList = new ConditionList();
        var firstBurningCondition = CreateCondition(ConditionType.Burning);
        var secondBurningCondition = CreateCondition(ConditionType.Burning);
        var drunkCondition = CreateCondition(ConditionType.Drunk);

        conditionList.Add(firstBurningCondition);
        conditionList.Add(secondBurningCondition);
        conditionList.Add(drunkCondition);
        conditionList.GetAll();

        conditionList.RemoveByType(ConditionType.Burning);

        conditionList.Count.Should().Be(1);
        conditionList.GetByType(ConditionType.Burning).Should().BeEmpty();
        conditionList.GetFirstConditionOfType(ConditionType.Burning).Should().BeNull();
        conditionList.HasAnyConditionOf(ConditionType.Burning).Should().BeFalse();
        conditionList.HasAnyConditionOf(ConditionType.Burning, out IReadOnlyList<ICondition> burningConditions).Should().BeFalse();
        burningConditions.Should().BeEmpty();
        conditionList.GetByType(ConditionType.Drunk).Should().ContainSingle().Which.Should().Be(drunkCondition);
    }

    [Fact]
    public void Clear_empties_conditions_and_resets_cached_results()
    {
        var conditionList = new ConditionList();
        var burningCondition = CreateCondition(ConditionType.Burning);
        var drunkCondition = CreateCondition(ConditionType.Drunk);

        conditionList.Add(burningCondition);
        conditionList.Add(drunkCondition);
        conditionList.GetAll();

        conditionList.Clear();

        conditionList.Count.Should().Be(0);
        conditionList.GetAll().Should().BeEmpty();
        conditionList.ToList().Should().BeEmpty();
        ((IEnumerable)conditionList).Cast<ICondition>().Should().BeEmpty();
    }

    [Fact]
    public void GetAll_allows_removing_a_not_yet_processed_condition_while_iterating_without_throwing()
    {
        var conditionList = new ConditionList();
        var firstCondition = CreateCondition(ConditionType.Burning);
        var secondCondition = CreateCondition(ConditionType.Drunk);
        var visitedConditions = new List<ICondition>();

        conditionList.Add(firstCondition);
        conditionList.Add(secondCondition);

        conditionList.Invoking(x =>
        {
            foreach (var condition in x.GetAll())
            {
                visitedConditions.Add(condition);

                if (condition == firstCondition)
                {
                    x.Remove(secondCondition);
                }
            }
        }).Should().NotThrow();

        visitedConditions.Should().Equal(firstCondition, secondCondition);
        conditionList.Count.Should().Be(1);
        conditionList.GetAll().Should().ContainSingle().Which.Should().Be(firstCondition);
    }

    [Fact]
    public void Add_accumulates_conditions_of_same_type()
    {
        var conditionList = new ConditionList();
        var originalCondition = CreateCondition(ConditionType.Burning);
        var replacementCondition = CreateCondition(ConditionType.Burning);
        var drunkCondition = CreateCondition(ConditionType.Drunk);

        conditionList.Add(originalCondition);
        conditionList.Add(drunkCondition);
        conditionList.Add(replacementCondition);

        conditionList.Count.Should().Be(3);
        conditionList.GetByType(ConditionType.Burning).Should().HaveCount(2);
        conditionList.GetByType(ConditionType.Drunk).Should().ContainSingle().Which.Should().Be(drunkCondition);
    }

    [Fact]
    public void Removing_using_Remove_removes_condition_from_list()
    {
        var conditionList = new ConditionList();
        var burningCondition = CreateCondition(ConditionType.Burning);
        var drunkCondition = CreateCondition(ConditionType.Drunk);

        conditionList.Add(burningCondition);
        conditionList.Add(drunkCondition);

        conditionList.Remove(burningCondition);

        conditionList.Count.Should().Be(1);
        conditionList.GetAll().Should().OnlyContain(condition => condition.Type == ConditionType.Drunk);
    }

    [Fact]
    public void Removing_using_RemoveByType_removes_all_matching_conditions()
    {
        var conditionList = new ConditionList();
        var burningCondition = CreateCondition(ConditionType.Burning);
        var drunkCondition = CreateCondition(ConditionType.Drunk);

        conditionList.Add(burningCondition);
        conditionList.Add(drunkCondition);

        conditionList.RemoveByType(ConditionType.Burning);

        conditionList.Count.Should().Be(1);
        conditionList.GetAll().Should().OnlyContain(condition => condition.Type == ConditionType.Drunk);
    }

    [Fact]
    public void Clear_does_not_throw()
    {
        var conditionList = new ConditionList();
        var burningCondition = CreateCondition(ConditionType.Burning);

        conditionList.Add(burningCondition);

        conditionList.Invoking(x => x.Clear()).Should().NotThrow();
        conditionList.Count.Should().Be(0);
    }

    [Fact]
    public void Add_null_throws_ArgumentNullException()
    {
        var conditionList = new ConditionList();

        conditionList.Invoking(x => x.Add(null)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Clear_does_not_throw_when_called()
    {
        var conditionList = new ConditionList();
        var burningCondition = new Condition(ConditionType.Burning, 100);
        conditionList.Add(burningCondition);

        conditionList.Invoking(x => x.Clear()).Should().NotThrow();
        conditionList.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveNonPersistentByType_removes_non_persistent_conditions()
    {
        var conditionList = new ConditionList();
        conditionList.Add(new Condition(ConditionType.Burning, 100));
        conditionList.Add(new Condition(ConditionType.Burning, 200));

        conditionList.RemoveNonPersistentByType(ConditionType.Burning);

        conditionList.HasAnyConditionOf(ConditionType.Burning).Should().BeFalse();
    }

    [Fact]
    public void RemoveNonPersistentByType_keeps_persistent_conditions()
    {
        var conditionList = new ConditionList();
        conditionList.Add(new Condition(ConditionType.Burning, 0));
        conditionList.Add(new Condition(ConditionType.Burning, 100));

        conditionList.RemoveNonPersistentByType(ConditionType.Burning);

        conditionList.HasAnyConditionOf(ConditionType.Burning).Should().BeTrue();
        conditionList.GetFirstConditionOfType(ConditionType.Burning).IsPersistent.Should().BeTrue();
    }

    [Fact]
    public void RemoveNonPersistentByType_does_not_affect_other_types()
    {
        var conditionList = new ConditionList();
        conditionList.Add(new Condition(ConditionType.Burning, 100));
        conditionList.Add(new Condition(ConditionType.Drunk, 100));

        conditionList.RemoveNonPersistentByType(ConditionType.Burning);

        conditionList.HasAnyConditionOf(ConditionType.Drunk).Should().BeTrue();
    }

    [Fact]
    public void RemoveNonPersistentByType_does_nothing_when_no_conditions_of_type()
    {
        var conditionList = new ConditionList();

        conditionList.Invoking(x => x.RemoveNonPersistentByType(ConditionType.Drunk)).Should().NotThrow();
    }

    [Fact]
    public void RemoveNonPersistentByType_does_nothing_when_all_persistent()
    {
        var conditionList = new ConditionList();
        conditionList.Add(new Condition(ConditionType.Burning, 0));

        conditionList.RemoveNonPersistentByType(ConditionType.Burning);

        conditionList.HasAnyConditionOf(ConditionType.Burning).Should().BeTrue();
    }

    private static Condition CreateCondition(ConditionType type, uint duration = 100) =>
        new(type, duration);
}