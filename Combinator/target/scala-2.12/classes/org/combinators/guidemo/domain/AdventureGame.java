package org.combinators.guidemo.domain;

import java.net.URL;
import java.util.*;

public class AdventureGame {
    private AbilityTypes ability;
    private BossAbilityTypes boss;
    private ArrayList<WeatherTypes> weather;

    public AbilityTypes getAbility() {
        return ability;
    }
    public void setAbility(AbilityTypes ability) {
        this.ability = ability;
    }

    public List<WeatherTypes> getWeather() {
        return weather;
    }
    public void setWeather(ArrayList<WeatherTypes> weather) {
        this.weather = weather;
    }

    public BossAbilityTypes getBoss() {
        return boss;
    }
    public void setBoss(BossAbilityTypes boss) {
        this.boss = boss;
    }
}