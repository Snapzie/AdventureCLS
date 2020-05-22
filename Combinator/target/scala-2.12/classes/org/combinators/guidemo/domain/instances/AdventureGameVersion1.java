package org.combinators.guidemo.domain.instances;

import org.combinators.guidemo.domain.AdventureGame;
import org.combinators.guidemo.domain.AbilityTypes;
import org.combinators.guidemo.domain.WeatherTypes;
import org.combinators.guidemo.domain.BossAbilityTypes;
import java.util.*;
import java.util.ArrayList;
import java.util.List;

public class AdventureGameVersion1 extends AdventureGame {
    public AdventureGameVersion1() {
        super.setAbility(AbilityTypes.fireball);
        super.setBoss(BossAbilityTypes.heal);
        super.setWeather(new ArrayList<WeatherTypes>(Arrays.asList(WeatherTypes.Blizzard, WeatherTypes.Sunny)));
    }
}