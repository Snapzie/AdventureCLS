package org.combinators.guidemo.domain.instances;

//import org.combinators.guidemo.domain.AdventureGame;
import org.combinators.guidemo.domain.*;

import java.util.*;

public class AdventureGameVersion1 extends AdventureGame {
    public AdventureGameVersion1() {
        super.setAbility(AbilityTypes.none);
        super.setWeather(new ArrayList<>(Arrays.asList(WeatherTypes.Night, WeatherTypes.Blizzard))); //Arrays.asList(WeatherTypes.Cloudy)
    }
}