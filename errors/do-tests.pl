#!/usr/bin/perl -w

unless ($#ARGV == 2) {
    print STDERR "Usage: $0 profile compiler glob-pattern\n";
    exit 1;
}

#
# Expected value constants
#
my $EXPECTING_WRONG_ERROR = 1;
my $EXPECTING_NO_ERROR    = 2;
my %expecting_map = ();

my $profile = $ARGV [0];
my $compile = $ARGV [1];
my $files   = $ARGV [2];

if (open (EXPECT_WRONG, "<$profile-expect-wrong-error")) {
	$expecting_map{$_} = $EXPECTING_WRONG_ERROR 
	foreach map {
		chomp,                     # remove trailing \n
		s/\#.*//g,                 # remove # style comments
		s/ //g;                    # remove whitespace
		$_ eq "" ? () : $_;        # now copy over non empty stuff
	} <EXPECT_WRONG>;
	
	close EXPECT_WRONG;
}

if (open (EXPECT_NO, "<$profile-expect-no-error")) {
	$expecting_map{$_} = $EXPECTING_NO_ERROR 
	foreach map {
		chomp,                     # remove trailing \n
		s/\#.*//g,                 # remove # style comments
		s/ //g;                    # remove whitespace
		$_ eq "" ? () : $_;        # now copy over non empty stuff
	} <EXPECT_NO>;
	
	close EXPECT_NO;
}
my $RESULT_UNEXPECTED_CORRECT_ERROR     = 1;
my $RESULT_CORRECT_ERROR                = 2;
my $RESULT_UNEXPECTED_INCORRECT_ERROR   = 3;
my $RESULT_EXPECTED_INCORRECT_ERROR     = 4;
my $RESULT_UNEXPECTED_NO_ERROR          = 5;
my $RESULT_EXPECTED_NO_ERROR            = 6;

my @statuses = (
	"UNEXPECTED SUCCESS",
	"SUCCESS",
	"UNEXPECTED INCORRECT ERROR",
	"INCORRECT ERROR",
	"UNEXPECTED NO ERROR",
	"NO ERROR",
);

my @status_items = (
	[],
	[],
	[],
	[],
	[],
	[],
);

my %results_map = ();

foreach (glob ($files)) {

	print "$_...";
	my ($error_number) = (/[a-z]*(\d+)(-\d+)?\.cs/);
	my $options = `fgrep "// Compiler options:" $_ | sed -e 's/\\/\\/ Compiler options://'`;
	system "$compile $_ --expect-error $error_number $options > /dev/null";
	
	exit 1 if $? & 127;
	
	my $exit_value = $? >> 8;
	
	die "unexpected return from mcs" if $exit_value > 2;
	
	my $status;
	
	if ($exit_value == 0) {
		$status = $RESULT_UNEXPECTED_CORRECT_ERROR     if     exists $expecting_map {$_};
		$status = $RESULT_CORRECT_ERROR                unless exists $expecting_map {$_};
	}
	
	if ($exit_value == 1) {
		$status = $RESULT_UNEXPECTED_INCORRECT_ERROR   unless exists $expecting_map {$_} and $expecting_map {$_} == $EXPECTING_WRONG_ERROR;
		$status = $RESULT_EXPECTED_INCORRECT_ERROR     if     exists $expecting_map {$_} and $expecting_map {$_} == $EXPECTING_WRONG_ERROR;
	}
	
	if ($exit_value == 2) {
		$status = $RESULT_UNEXPECTED_NO_ERROR          unless exists $expecting_map {$_} and $expecting_map {$_} == $EXPECTING_NO_ERROR;
		$status = $RESULT_EXPECTED_NO_ERROR            if     exists $expecting_map {$_} and $expecting_map {$_} == $EXPECTING_NO_ERROR;
	}
	

	push @{@status_items [($status - 1)]}, $_;
	print "@statuses[($status - 1)]\n";
	$results_map{$_} = $status;
}
print scalar @{@status_items [($RESULT_CORRECT_ERROR              - 1)]}, " Correct errors\n";
print scalar @{@status_items [($RESULT_EXPECTED_INCORRECT_ERROR   - 1)]}, " Incorrect errors\n";
print scalar @{@status_items [($RESULT_EXPECTED_NO_ERROR          - 1)]}, " No errors\n";

print scalar @{@status_items [($RESULT_UNEXPECTED_CORRECT_ERROR   - 1)]}, " Unexpected correct errors\n";
print, print "\n" foreach @{@status_items [($RESULT_UNEXPECTED_CORRECT_ERROR - 1)]};

print scalar @{@status_items [($RESULT_UNEXPECTED_INCORRECT_ERROR - 1)]}, " Unexpected incorrect errors\n";
print, print "\n" foreach @{@status_items [($RESULT_UNEXPECTED_INCORRECT_ERROR - 1)]};

print scalar @{@status_items [($RESULT_UNEXPECTED_NO_ERROR        - 1)]}, " Unexpected no errors\n";
print, print "\n" foreach @{@status_items [($RESULT_UNEXPECTED_NO_ERROR - 1)]};

print "\n";

print "OVERALL:";
print scalar @{@status_items [($RESULT_CORRECT_ERROR - 1)]}, " tests succeeded\n";
print scalar @{@status_items [($RESULT_EXPECTED_INCORRECT_ERROR - 1)]} + scalar @{@status_items [($RESULT_EXPECTED_NO_ERROR - 1)]}, " known errors\n";
print scalar @{@status_items [($RESULT_UNEXPECTED_INCORRECT_ERROR - 1)]} + scalar @{@status_items [($RESULT_UNEXPECTED_NO_ERROR - 1)]}, " errors\n";
print scalar @{@status_items [($RESULT_UNEXPECTED_CORRECT_ERROR   - 1)]}, " new tests passing\n";

exit (
	scalar @{@status_items [($RESULT_UNEXPECTED_INCORRECT_ERROR - 1)]} +
	scalar @{@status_items [($RESULT_UNEXPECTED_NO_ERROR        - 1)]} +
	scalar @{@status_items [($RESULT_UNEXPECTED_CORRECT_ERROR   - 1)]}
) == 0 ? 0 : 1;
