package org.combinators.guidemo

case class MyResult(var theCode: String, filename: String)

import org.combinators.templating.persistable.Persistable
import _root_.java.nio.file.{FileAlreadyExistsException, Files, Path}
import java.io.File

class MyResultPersistable extends Persistable {
    type T = MyResult

    def rawText(elem: T): Array[Byte] =
        elem.theCode.getBytes

    def path(elem: T): Path =
        java.nio.file.Paths.get(elem.filename)
}