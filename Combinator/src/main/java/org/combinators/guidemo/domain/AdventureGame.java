package org.combinators.guidemo.domain;

import java.net.URL;
import java.util.*;

public class AdventureGame {
    private AbilityTypes ability;
    private List<WeatherTypes> weather;

    public AbilityTypes getAbility() {
        return ability;
    }
    public void setAbility(AbilityTypes ability) {
        this.ability = ability;
    }

    public List<WeatherTypes> getWeather() {
        return weather;
    }
    public void setWeather(List<WeatherTypes> weather) {
        this.weather = weather;
    }
}