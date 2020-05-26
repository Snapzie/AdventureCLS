package org.combinators.guidemo

import java.nio.file.{Files, Paths}
import javax.inject.Inject

import play.api.mvc.{Action, InjectedController}
import org.combinators.guidemo.Helpers._
import org.combinators.cls.interpreter.CombinatorInfo
import org.combinators.cls.types.{Omega, Type}
import org.combinators.cls.git.{RoutingEntries, EmptyResults, InhabitationController, Results, EmptyInhabitationBatchJobResults}
import org.combinators.templating.persistable.JavaPersistable._
import org.combinators.cls.types.syntax._
import org.webjars.play.{RequireJS, WebJarsUtil}
import play.api.inject.ApplicationLifecycle

import org.combinators.guidemo.domain.AdventureGame
import org.combinators.guidemo.domain.instances.AdventureGameVersion1

import org.combinators.templating.persistable.{BundledResource, Persistable}

abstract class AdventureGameController (webJars: WebJarsUtil, lifeCycle: ApplicationLifecycle) 
    extends InhabitationController(webJars, lifeCycle)
    with RoutingEntries {
    val adventureGame: AdventureGame
    lazy val repository = new Repository(adventureGame)
    lazy val Gamma = repository.forInhabitation
    override lazy val combinatorComponents: Map[String, CombinatorInfo] = Gamma.combinatorComponents
    implicit val myResultPersistable: Persistable.Aux[MyResult] =
        new MyResultPersistable
    override lazy val results: Results = EmptyInhabitationBatchJobResults(repository.forInhabitation)
        //.addJob[MyResult](repository.semanticPlayerTarget) 
        //.addJob[MyResult](repository.semanticPlayerTestTarget)
        //.addJob[MyResult](repository.semanticAdventureTarget)
        //.addJob[MyResult](repository.semanticBossTarget)
        //.addJob[MyResult](repository.semanticBossTestTarget)
        .addJob[MyResult](repository.semanticRoomTarget)
        //.addJob[MyResult](repository.semanticRoomTestTarget)
        //.addJob[MyResult](repository.semanticPlayerIntegrationTarget)
        //.addJob[MyResult](repository.semanticBossIntegrationTarget)
        .compute()
    override lazy val controllerAddress: String = adventureGame.getClass.getSimpleName.toLowerCase
}

class AdventureGameVersion1Controller @Inject()(webJars: WebJarsUtil, lifeCycle: ApplicationLifecycle) 
    extends AdventureGameController(webJars, lifeCycle) {
    lazy val adventureGame: AdventureGame = new AdventureGameVersion1()
}

// class PersonalGreetingController @Inject()(webJars: WebJarsUtil, lifeCycle: ApplicationLifecycle) 
//     extends GreetingController(webJars, lifeCycle) {
//     lazy val greeting: Greeting = new PersonalGreeting()
// }
