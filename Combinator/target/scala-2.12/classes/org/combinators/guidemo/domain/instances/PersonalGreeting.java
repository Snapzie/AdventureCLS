package org.combinators.guidemo.domain.instances;

import org.combinators.guidemo.domain.Greeting;
import org.combinators.guidemo.domain.GreetingTypes;

public class PersonalGreeting extends Greeting {
    public PersonalGreeting() {
        super.setGreeting(GreetingTypes.personal);
    }
}