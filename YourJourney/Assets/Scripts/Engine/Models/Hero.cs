using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using static LanguageManager;

public enum Sex { NONE, MALE, FEMALE };
public enum Race { NONE, ELF, MAN, DWARF, HALFLING, WIZARD, WEREBEAR };
public class Hero
{
    public int id;
    public string name;
    public Sex sex;
    public Race race;
    public int maxHanded = 2;

    //Suggested starting role
    public Role role;

    //Suggested starting equipment
    public ItemSeries armor;
    public ItemSeries hand1;
    public ItemSeries hand2;
    public ItemSeries trinket;
    public ItemSeries mount;

    public Hero() {}

    public Hero(int id)
    {
        this.id = id;
    }
}
