using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Skills;
using System.Reflection;

namespace Uzi.Ikosa
{
    public class SkillLister : IFeatParameterProvider
    {
        public IEnumerable<FeatParameter> AvailableParameters(ParameterizedFeatListItem target, Creature creature, int powerDie)
        {
            foreach (Type _type in Assembly.GetAssembly(typeof(SkillBase)).GetTypes())
            {
                if (_type.IsSubclassOf(typeof(SkillBase)) 
                    && (!_type.IsAbstract) 
                    && (_type.IsPublic))
                {
                    // get skill info
                    SkillInfoAttribute _info = SkillBase.SkillInfo(_type);
                    string _name;
                    string _description;
                    if (_info != null)
                    {
                        _name = _info.Name;
                        _description = string.Format("{0}{1}{2}", _info.Mnemonic, (_info.UseUntrained ? "; Untrained" : ""),
                                (_info.CheckFactor > 0d ? string.Format("; Check*{0}", _info.CheckFactor) : ""));
                    }
                    else
                    {
                        _name = _type.Name;
                        _description = "Skill";
                    }
                    if (_type.IsGenericTypeDefinition)
                    {
                        // skill with sub skills
                        // find the constraints of the generic type parameter, build an enumerator to return them
                        Type[] _args = _type.GetGenericArguments();
                        Type[] _constraints = _args[0].GetGenericParameterConstraints();
                        var _subList = (from _focus in Assembly.GetAssembly(typeof(SkillBase)).GetTypes()
                                        where _focus.IsPublic && !_focus.IsAbstract && _focus.IsSubclassOf(_constraints[0])
                                        select new FeatParameter(_focus, SkillBase.SkillInfo(_focus).Name, string.Empty, powerDie))
                                        .ToList();
                        yield return new ParameterizedFeatParameter(target, _type, _name, _description, powerDie, _subList);
                    }
                    else
                    {
                        // simple skill
                        yield return new FeatParameter(target, _type, _name, _description, powerDie);
                    }
                }
            }
            yield break;
        }
    }
}
