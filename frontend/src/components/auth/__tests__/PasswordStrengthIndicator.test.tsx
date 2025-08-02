import React from 'react';
import { render, screen } from '@testing-library/react';
import { vi } from 'vitest';
import PasswordStrengthIndicator from '../PasswordStrengthIndicator';

vi.mock('@/hooks/useLocalization', () => ({
  useLocalization: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'auth.passwordStrength': 'Password Strength',
        'auth.weak': 'Weak',
        'auth.medium': 'Medium',
        'auth.strong': 'Strong',
        'auth.passwordMustContain': 'Password must contain',
        'auth.passwordReq.minLength': 'At least 8 characters',
        'auth.passwordReq.uppercase': 'One uppercase letter',
        'auth.passwordReq.lowercase': 'One lowercase letter',
        'auth.passwordReq.number': 'One number',
        'common.completed': 'Completed',
        'common.pending': 'Pending',
      };
      return translations[key] || key;
    },
  }),
}));

describe('PasswordStrengthIndicator', () => {
  it('does not render anything for empty password', () => {
    const { container } = render(<PasswordStrengthIndicator password="" />);
    expect(container.firstChild?.firstChild).toBeNull();
  });

  it('shows weak strength for simple password', () => {
    render(<PasswordStrengthIndicator password="weak" />);
    
    expect(screen.getByText('Password Strength:')).toBeInTheDocument();
    expect(screen.getByText('Weak')).toBeInTheDocument();
  });

  it('shows medium strength for moderately complex password', () => {
    render(<PasswordStrengthIndicator password="Password" />);
    
    expect(screen.getByText('Medium')).toBeInTheDocument();
  });

  it('shows strong strength for complex password', () => {
    render(<PasswordStrengthIndicator password="StrongPassword123" />);
    
    expect(screen.getByText('Strong')).toBeInTheDocument();
  });

  it('displays password requirements by default', () => {
    render(<PasswordStrengthIndicator password="test" />);
    
    expect(screen.getByText('Password must contain:')).toBeInTheDocument();
    expect(screen.getByText('At least 8 characters')).toBeInTheDocument();
    expect(screen.getByText('One uppercase letter')).toBeInTheDocument();
    expect(screen.getByText('One lowercase letter')).toBeInTheDocument();
    expect(screen.getByText('One number')).toBeInTheDocument();
  });

  it('hides password requirements when showRequirements is false', () => {
    render(<PasswordStrengthIndicator password="test" showRequirements={false} />);
    
    expect(screen.queryByText('Password must contain:')).not.toBeInTheDocument();
    expect(screen.queryByText('At least 8 characters')).not.toBeInTheDocument();
  });

  it('marks requirements as completed when met', () => {
    render(<PasswordStrengthIndicator password="StrongPassword123" />);
    
    // All requirements should be marked as completed (with checkmarks)
    const requirements = screen.getAllByText(/✓/);
    expect(requirements).toHaveLength(4);
  });

  it('marks requirements as pending when not met', () => {
    render(<PasswordStrengthIndicator password="weak" />);
    
    // Most requirements should be pending (with circles)
    const pendingRequirements = screen.getAllByText(/○/);
    expect(pendingRequirements.length).toBeGreaterThan(0);
  });

  it('has proper accessibility attributes for progress bar', () => {
    render(<PasswordStrengthIndicator password="test" />);
    
    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toHaveAttribute('aria-valuenow');
    expect(progressBar).toHaveAttribute('aria-valuemin', '0');
    expect(progressBar).toHaveAttribute('aria-valuemax', '4');
    expect(progressBar).toHaveAttribute('aria-label');
  });

  it('provides screen reader text for requirement status', () => {
    render(<PasswordStrengthIndicator password="StrongPassword123" />);
    
    // Should have screen reader text for completed requirements
    expect(screen.getAllByText('Completed')).toHaveLength(4);
  });

  it('applies custom className', () => {
    const { container } = render(
      <PasswordStrengthIndicator password="test" className="custom-class" />
    );
    
    expect(container.firstChild).toHaveClass('custom-class');
  });

  it('updates strength indicator as password changes', () => {
    const { rerender } = render(<PasswordStrengthIndicator password="weak" />);
    expect(screen.getByText('Weak')).toBeInTheDocument();

    rerender(<PasswordStrengthIndicator password="StrongPassword123" />);
    expect(screen.getByText('Strong')).toBeInTheDocument();
    expect(screen.queryByText('Weak')).not.toBeInTheDocument();
  });

  it('shows correct progress bar width for different strengths', () => {
    const { rerender } = render(<PasswordStrengthIndicator password="weak" />);
    
    let progressBar = screen.getByRole('progressbar');
    expect(progressBar).toHaveStyle({ width: '25%' }); // 1/4 requirements met

    rerender(<PasswordStrengthIndicator password="StrongPassword123" />);
    progressBar = screen.getByRole('progressbar');
    expect(progressBar).toHaveStyle({ width: '100%' }); // 4/4 requirements met
  });
});