using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Domain.Creatures.Player;

public class Skill : ISkill
{
    private static readonly Dictionary<SkillType, byte> SkillOffsetMap = new()
    {
        [SkillType.Magic] = 0,
        [SkillType.Axe] = 10,
        [SkillType.Sword] = 10,
        [SkillType.Club] = 10,
        [SkillType.Fishing] = 10,
        [SkillType.Fist] = 10,
        [SkillType.Distance] = 10,
        [SkillType.Shielding] = 10
    };

    public Skill(SkillType type, ushort level = 0, double count = 0)
    {
        if (count < 0) throw new Exception($"{nameof(count)} cannot be negative.");
        Type = type;
        Level = level;
        Count = count;
    }

    public byte SkillOffset => SkillOffsetMap[Type];

    public event LevelAdvance OnAdvance;
    public event LevelRegress OnRegress;
    public event IncreaseSkillPoints OnIncreaseSkillPoints;

    public sbyte Bonus { get; private set; }

    public void AddBonus(sbyte increase)
    {
        Bonus = (sbyte)(Bonus + increase);
    }

    public void RemoveBonus(sbyte decrease)
    {
        Bonus = (sbyte)(Bonus - decrease);
    }

    public SkillType Type { get; }
    public ushort Level { get; private set; }
    public double Count { get; private set; }

    public Func<double> GetIncreaseRate { get; init; }

    public double GetPercentage(float rate)
    {
        return CalculatePercentage(Count, rate);
    }

    public void IncreaseCounter(double value, float rate)
    {
        if (rate < 0) throw new Exception($"{nameof(rate)} must be positive.");
        Count += value;

        if (Type == SkillType.Level)
        {
            IncreaseLevel();
            return;
        }

        IncreaseSkillLevel(rate);
    }

    public void DecreaseCounter(double value, float rate)
    {
        if (rate < 0)
            return;

        Count = value;
        if (Type == SkillType.Level) DecreaseLevel();
        else DecreaseSkillLevel(rate);
    }

    public void DecreaseLevel(double lostExperience)
    {
        if (Type != SkillType.Level) return;

        var oldLevel = Level;
        Count = Math.Max(Count - lostExperience, 0);

        while (Level > 1 && Count < CalculateExpByLevel(Level)) --Level;

        if (oldLevel != Level) OnRegress?.Invoke(Type, oldLevel, Level);
    }

    public void DecreaseLevel()
    {
        if (Type != SkillType.Level) return;

        var oldLevel = Level;
        while (Count < CalculateExpByLevel(Level)) Level--;

        if (oldLevel != Level) OnRegress?.Invoke(Type, oldLevel, Level);
    }

    public static double CalculateExpByLevel(int level)
    {
        return Math.Ceiling(50 * Math.Pow(level, 3) / 3 - 100 * Math.Pow(level, 2) + 850 * level / 3 - 200);
    }

    private double GetPointsForSkillLevel(int targetSkillLevel, float vocationRate)
    {
        if (Type == SkillType.Magic) return GetRequiredMana(targetSkillLevel, vocationRate);

        return Math.Pow(vocationRate, targetSkillLevel - SkillOffset) / GetIncreaseRate();
    }

    private static double CalculatePercentage(double count, double nextLevelCount)
    {
        return Math.Min(100, count * 100 / nextLevelCount);
    }

    private double CalculatePercentage(double count, float rate)
    {
        if (Type == SkillType.Level)
        {
            var currentLevelExp = CalculateExpByLevel(Level);
            var nextLevelExp = CalculateExpByLevel(Level + 1);

            if (count < currentLevelExp)
                count = currentLevelExp;
            if (count > nextLevelExp)
                count = nextLevelExp;

            Count = count;

            return CalculatePercentage(count - currentLevelExp, nextLevelExp - currentLevelExp);
        }

        if (Type == SkillType.Magic) return GetManaPercentage(count, rate);

        return CalculatePercentage(count, GetPointsForSkillLevel(Level + 1, rate));
    }

    private double GetManaPercentage(double manaSpent, float rate)
    {
        var requiredMana = GetRequiredMana(Level + 1, rate);

        return CalculatePercentage(manaSpent, requiredMana);
    }

    private double GetRequiredMana(int targetLevel, float vocationRate)
    {
        var reqMana = 1600 * Math.Pow(vocationRate, targetLevel);
        var modResult = reqMana % 20;
        if (modResult < 10)
            reqMana -= modResult;
        else
            reqMana -= modResult + 20;

        return reqMana;
    }

    public void IncreaseLevel()
    {
        if (Type != SkillType.Level) return;

        var oldLevel = Level;
        while (Count >= CalculateExpByLevel(Level + 1)) Level++;

        if (oldLevel != Level) OnAdvance?.Invoke(Type, oldLevel, Level);
    }

    public void IncreaseSkillLevel(float rate)
    {
        if (Type == SkillType.Level) return;

        var oldLevel = Level;
        while (Count >= GetPointsForSkillLevel(Level + 1, rate))
        {
            Count = 0;
            Level++;
        }

        if (oldLevel != Level) OnAdvance?.Invoke(Type, oldLevel, Level);

        OnIncreaseSkillPoints?.Invoke(Type);
    }

    public void DecreaseSkillLevel(float rate)
    {
        if (Type == SkillType.Level) return;

        var oldLevel = Level;
        while (Count < GetPointsForSkillLevel(Level, rate))
        {
            Count = 0;
            Level--;
        }

        if (oldLevel != Level) OnAdvance?.Invoke(Type, oldLevel, Level);

        OnIncreaseSkillPoints?.Invoke(Type);
    }
}