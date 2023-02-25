using System;

namespace Uzi.Ikosa.Languages
{
    [Serializable]
    public class Abyssal : Language
    {
        public Abyssal(object source) : base(source) { }
        public override string Alphabet { get { return @"Infernal"; } }
        public override Language GetCopy(object source) => new Abyssal(source);
    }

    [Serializable]
    public class Aquan : Language
    {
        public Aquan(object source) : base(source) { }
        public override string Alphabet { get { return @"Elven"; } }
        public override Language GetCopy(object source) => new Aquan(source);
    }

    [Serializable]
    public class Auran : Language
    {
        public Auran(object source) : base(source) { }
        public override string Alphabet { get { return @"Draconic"; } }
        public override Language GetCopy(object source) => new Auran(source);
    }

    [Serializable]
    public class Celestial : Language
    {
        public Celestial(object source) : base(source) { }
        public override string Alphabet { get { return @"Celestial"; } }
        public override Language GetCopy(object source) => new Celestial(source);
    }

    [Serializable]
    public class Common : Language
    {
        public Common(object source) : base(source) { }
        public override string Alphabet { get { return @"Common"; } }
        public override Language GetCopy(object source) => new Common(source);
    }

    [Serializable]
    public class Draconic : Language
    {
        public Draconic(object source) : base(source) { }
        public override string Alphabet { get { return @"Draconic"; } }
        public override Language GetCopy(object source) => new Draconic(source);
    }

    [Serializable]
    public class Druidic : Language
    {
        public Druidic(object source) : base(source) { }
        public override string Alphabet { get { return @"Druidic"; } }
        public override Language GetCopy(object source) => new Druidic(source);
    }

    [Serializable]
    public class Dwarven : Language
    {
        public Dwarven(object source) : base(source) { }
        public override string Alphabet { get { return @"Dwarven"; } }
        public override Language GetCopy(object source) => new Dwarven(source);
    }

    [Serializable]
    public class Elven : Language
    {
        public Elven(object source) : base(source) { }
        public override string Alphabet { get { return @"Elven"; } }
        public override Language GetCopy(object source) => new Elven(source);
    }

    [Serializable]
    public class Giant : Language
    {
        public Giant(object source) : base(source) { }
        public override string Alphabet { get { return @"Dwarven"; } }
        public override Language GetCopy(object source) => new Giant(source);
    }

    [Serializable]
    public class Gnome : Language
    {
        public Gnome(object source) : base(source) { }
        public override string Alphabet { get { return @"Dwarven"; } }
        public override Language GetCopy(object source) => new Gnome(source);
    }

    [Serializable]
    public class Goblin : Language
    {
        public Goblin(object source) : base(source) { }
        public override string Alphabet { get { return @"Dwarven"; } }
        public override Language GetCopy(object source) => new Goblin(source);
    }

    [Serializable]
    public class Gnollish : Language
    {
        public Gnollish(object source) : base(source) { }
        public override string Alphabet { get { return @"Common"; } }
        public override Language GetCopy(object source) => new Gnollish(source);
    }

    [Serializable]
    public class Grimlockese : Language
    {
        public Grimlockese(object source) : base(source) { }
        public override string Alphabet { get { return @"Dwarven"; } }
        public override Language GetCopy(object source) => new Grimlockese(source);
    }

    [Serializable]
    public class Halfling : Language
    {
        public Halfling(object source) : base(source) { }
        public override string Alphabet { get { return @"Common"; } }
        public override Language GetCopy(object source) => new Halfling(source);
    }

    [Serializable]
    public class Ignan : Language
    {
        public Ignan(object source) : base(source) { }
        public override string Alphabet { get { return @"Draconic"; } }
        public override Language GetCopy(object source) => new Ignan(source);
    }

    [Serializable]
    public class Infernal : Language
    {
        public Infernal(object source) : base(source) { }
        public override string Alphabet { get { return @"Infernal"; } }
        public override Language GetCopy(object source) => new Infernal(source);
    }

    [Serializable]
    public class Orcish : Language
    {
        public Orcish(object source) : base(source) { }
        public override string Alphabet { get { return @"Dwarven"; } }
        public override Language GetCopy(object source) => new Orcish(source);
    }

    [Serializable]
    public class Sylvan : Language
    {
        public Sylvan(object source) : base(source) { }
        public override string Alphabet { get { return @"Elven"; } }
        public override Language GetCopy(object source) => new Sylvan(source);
    }

    [Serializable]
    public class Terran : Language
    {
        public Terran(object source) : base(source) { }
        public override string Alphabet { get { return @"Dwarven"; } }
        public override Language GetCopy(object source) => new Terran(source);
    }

    [Serializable]
    public class Undercommon : Language
    {
        public Undercommon(object source) : base(source) { }
        public override string Alphabet { get { return @"Elven"; } }
        public override Language GetCopy(object source) => new Undercommon(source);
    }

    [Serializable]
    public class Worg : Language
    {
        public Worg(object source) : base(source) { }
        public override string Alphabet { get { return @"-"; } }
        public override Language GetCopy(object source) => new Worg(source);
    }

    [Serializable]
    public class UnknownLanguage : Language
    {
        public UnknownLanguage(object source) : base(source) { }
        public override string Alphabet { get { return @"-"; } }
        public override Language GetCopy(object source) => new UnknownLanguage(source);
    }
}