package org.combinators.guidemo

import java.io.InputStream
import java.nio.file.Paths

import com.github.javaparser.ast.CompilationUnit
import org.combinators.templating.persistable.{BundledResource, ResourcePersistable}
import org.combinators.cls.git.{InhabitationController, Results}
import org.combinators.templating.twirl.Java

import java.io._

object Helpers {
  implicit val persistable: ResourcePersistable.Aux = ResourcePersistable.apply
  type Form = CompilationUnit
  type OptionSelection = Form => Runnable
  
  def addArbCode(file: MyResult, insertion: String, findName: String, afterChar: Char): Unit = {
    val pos = file.theCode.indexOf(afterChar, file.theCode.indexOf(findName))
    file.theCode = insertString(file.theCode, insertion, pos)
  }

    def insertString(originalString: String, stringToBeInserted: String, index: Int): String = { 
        // Create a new string 
        var newString = new String(); 

        for (i <- 0 to originalString.length() - 1) { 

            // Insert the original string character 
            // into the new string 
            newString += originalString.charAt(i); 
            if (i == index) { 
                // Insert the string to be inserted 
                // into the new string 
                newString += stringToBeInserted; 
            } 
        } 

        // return the modified String 
        newString; 
    }

  def readFile(name: String): String =
    scala.io.Source.fromInputStream(getClass.getResourceAsStream(name)).mkString
}
