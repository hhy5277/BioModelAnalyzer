// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
#ifndef VARIABLE_H_
#define VARIABLE_H_

#include <string>
#include <iosfwd>
#include "Type.h"
#include "EnumType.h"

class Variable
{
public:
	Variable()=delete;
	Variable(const Variable&);
	Variable(Variable&&);
	Variable& operator=(const Variable&) = delete;
	Variable& operator=(Variable&&) = delete;

	Variable(const std::string&, bool val); // Create a Bool Variable
	Variable(const std::string&/*, const Type& t*/, Type::Value*); // Create an Enumerated 
	~Variable();

	void set(bool val);
	void set(const Type::Value&);

	const Type::Value* value() const;
	bool operator==(const Variable& other) const;
	bool operator!=(const Variable& other) const;

	friend std::ostream& operator<<(std::ostream&, const Variable&);
private:
	std::string _name;
	const Type& _type; // Point to the type owned by someone else
	Type::Value* _val; // Own this
};


#endif
