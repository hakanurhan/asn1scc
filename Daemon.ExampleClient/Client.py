#!/usr/bin/env python
import requests


def get_ast(uri, data):
    print("------------------")
    print("Requesting AST XML for contents: ", data['AsnFiles'][0]['Contents'])
    print("Response:", requests.post(uri + "ast", json = data).json())


def get_correct_ast(uri):
    data = {
        'AsnFiles': [
                {
                    'Name': 'Test.asn',
                    'Contents': 'Example DEFINITIONS ::= BEGIN MyInt ::= INTEGER(0 .. 10) END'
                }
        ],
        'AcnFiles': []
    }
    get_ast(uri, data)
    
def get_correct_ast_with_acn(uri):
    data = {
        'AsnFiles': [
                {
                    'Name': 'Test.asn',
                    'Contents': 'Example DEFINITIONS ::= BEGIN MyInt ::= INTEGER(0 .. 10) END'
                }
        ],
        'AcnFiles': [
                {
                    'Name': 'Test.acn',
                    'Contents': 'Example DEFINITIONS ::= BEGIN MyInt [size 32, encoding pos-int] END'
                }
        ]
    }
    get_ast(uri, data)

def get_compilation_error(uri):
    data = {
        'AsnFiles': [
                {
                    'Name': 'Bad.asn',
                    'Contents': 'Example DEFINITIONS ::= END'
                }
        ],
        'AcnFiles': []
    }
    get_ast(uri, data)
    
def get_ast_with_options(uri):
    data = {
        'AsnFiles': [
                {
                    'Name': 'Test.asn',
                    'Contents': 'Example DEFINITIONS ::= BEGIN MyInt ::= INTEGER(0 .. 10) MySequence ::= SEQUENCE { a MyInt } END'
                }
        ],
        'AcnFiles': [
                {
                    'Name': 'Test.acn',
                    'Contents': 'Example DEFINITIONS ::= BEGIN MyInt [size 32, encoding pos-int] END'
                }
        ],
        'Options' : {
            'TypePrefix' : 'T',
            'Encoding' : { 'Case' : 'BER' },
            'FieldPrefix' : { 'Case' : 'FieldPrefixUserValue', 'Fields' : ['PREF_']}
        }
    }
    get_ast(uri, data)

def run(uri):
    print("asn1scc Daemon Test Client")
    print("asn1scc Daemon version:", requests.get(uri + "version").content)

    get_correct_ast(uri)
    get_compilation_error(uri)
    get_correct_ast_with_acn(uri)
    get_ast_with_options(uri)


if __name__ == "__main__":
    import sys
    uri = sys.argv[1] if len(sys.argv) > 1 else "http://localhost:9749/"
    run(uri)
