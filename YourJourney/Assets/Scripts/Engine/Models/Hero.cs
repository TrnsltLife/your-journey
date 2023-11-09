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
    public Sex sex;
    public Race race;

    public Hero()
    {
    }
}
