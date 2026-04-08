using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkill
{
    void Activate();
}

public class FireballSkill : ISkill
{
    public void Activate()
    {
        Debug.Log("释放火球，造成范围伤害");
    
    }
}
public class HealSkill : ISkill
{
    public void Activate()
    {
       
        Debug.Log("回复目标生命值");
    }
}
public class SkillManager
{
    public List<ISkill>_skills = new List<ISkill>();
    public  void AddSkill(ISkill skill)
    {
        _skills.Add(skill);

    }
    public void UseAllSkills()
    {
        _skills.ForEach(s => s.Activate());
    }

   
}
class InterfaceTest : MonoBehaviour
{
    private void Start()
    {


        SkillManager manager1 = new SkillManager();

        manager1.AddSkill(new FireballSkill());
        manager1.AddSkill(new HealSkill());
        manager1.UseAllSkills();
    }
}