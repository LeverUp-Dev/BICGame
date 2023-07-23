using System;

namespace Hypocrites.DB.Save
{
    using DB.Data;
    using Skill;

    [Serializable]
    public class BeingSave
    {
        public string name;

        public StatusSave status;
        public StatusSave additionalStatus;

        public string skillRemoveEventPath;
        public string[] effects;

        public BeingSave()
        {

        }

        public BeingSave(Being data)
        {
            name = data.Name;

            status.Load(data.Status);
            additionalStatus.Load(data.AdditionalStatus);

            skillRemoveEventPath = data.SkillRemoveEvent.path;
            effects = new string[data.Effects.Count];

            int i = 0;
            foreach (Skill effect in data.Effects)
                effects[++i] = effect.Name;
        }
    }
}
