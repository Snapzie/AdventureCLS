package org.combinators.guidemo.domain.instances;

import org.combinators.guidemo.domain.AdventureGame;
import org.combinators.guidemo.domain.Advenums;

public class AdventureGameVersion1 extends AdventureGame {
    public AdventureGameVersion1() {
        super.setAbility(AbilityTypes.none);
        super.setWeather(List.of(WeatherTypes.Blizzard, WeatherTypes.Sunny))
    }
}