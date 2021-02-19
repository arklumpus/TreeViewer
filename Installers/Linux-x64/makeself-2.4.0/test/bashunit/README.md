# bashunit

`bashunit` is a unit testing framework for Bash scripts based on xUnit principles.

This is similar to the [ShUnit](http://shunit.sourceforge.net/) and its
successor [shUnit2](https://code.google.com/p/shunit2/).

## Usage

Functions starting with 'test' will be automatically evaluated.

**1. Write test cases**

```bash
testEcho() {
    assertEqual "$(echo foo)" "foo"
    assertReturn "$(echo foo)" 0
}
```

**2. Include this script at the end of your test script**

```bash
source $(dirname $0)/bashunit.bash

# eof
```

**3. Run test suite**

```bash
$ ./test_example
testEcho:4:Passed
testEcho:5:Passed
Done. 2 passed. 0 failed. 0 skipped.
```

The return code is equal to the amount of failed testcases.

Options can be given to the test script:

```bash
$ bash ./bashunit.bash
Usage: <testscript> [options...]

Options:
  -v, --verbose  Print exptected and provided values
  -s, --summary  Only print summary omitting individual test results
  -q, --quiet    Do not print anything to standard output
  -h, --help     Show usage screen
```

## Dependencies

* Bash (`BASH_LINENO`)
* Shell colours

## API

* `assert($1)`

    `$1`: Expression

    Assert that a given expression evaluates to true.

* `assertEqual($1, $2)`

    `$1`: Output

    `$2`: Expected

    Assert that a given output string is equal to an expected string.

* `assertNotEqual($1, $2)`

    `$1`: Output

    `$2`: Expected

    Assert that a given output string is not equal to an expected string.

* `assertStartsWith($1, $2)`

    `$1`: Output

    `$2`: Expected

    Assert that a given output string starts with an expected string.

* `assertReturn($1, $2)`

    `$1`: Output

    `$2`: Expected

    `$?`: Provided

    Assert that the last command's return code is equal to an expected integer.

* `assertNotReturn($1, $2)`

    `$1`: Output

    `$2`: Expected

    `$?`: Provided

    Assert that the last command's return code is not equal to an expected
    integer.

* `assertGreaterThan($1, $2)`

    `$1` Output

    `$2` Expected

    Assert that a given integer is greater than an expected integer.

* `assertAtLeast($1, $2)`

    `$1` Output

    `$2` Expected

    Assert that a given integer is greater than or equal to an expected integer.

* `assertLessThan($1, $2)`

    `$1` Output

    `$2` Expected

    Assert that a given integer is less than an expected integer.

* `assertAtMost($1, $2)`

    `$1` Output

    `$2` Expected

    Assert that a given integer is less than or equal to an expected integer.

* `skip()`

    Skip the current test case.

## License

`bashunit` is licenced under a
[BSD License](https://github.com/djui/bashunit/blob/master/LICENSE).
